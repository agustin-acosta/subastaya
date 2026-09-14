using Application.Interfaces;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Workers;

public class LiquidacionWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LiquidacionWorker> _logger;
    private static readonly TimeSpan Intervalo = TimeSpan.FromSeconds(30);

    public LiquidacionWorker(IServiceScopeFactory scopeFactory, ILogger<LiquidacionWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var subastaRepository = scope.ServiceProvider.GetRequiredService<ISubastaRepository>();
                var billeteraRepository = scope.ServiceProvider.GetRequiredService<IBilleteraRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var ahora = DateTime.UtcNow;

                List<Subasta> vencidas;
                try
                {
                    vencidas = await subastaRepository.ObtenerVencidasSinLiquidarAsync(ahora, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al consultar subastas vencidas");
                    vencidas = new List<Subasta>();
                }

                foreach (var subasta in vencidas)
                {
                    try
                    {
                        var pujaGanadora = await subastaRepository.ObtenerPujaConMayorMontoAsync(subasta.Id, stoppingToken);

                        if (pujaGanadora is null)
                        {
                            subasta.Estado = EstadoSubasta.Desierta;

                            subastaRepository.AgregarAuditoria(new AuditoriaLog
                            {
                                Entidad = "Subasta",
                                EntidadId = subasta.Id,
                                Accion = "CIERRE_DESIERTA",
                                UsuarioId = null,
                                DetalleJson = "{}",
                                Fecha = ahora
                            });
                        }
                        else
                        {
                            subasta.Estado = EstadoSubasta.Finalizada;

                            var billeteraComprador = await billeteraRepository.ObtenerPorUsuarioIdAsync(pujaGanadora.CompradorId, stoppingToken);
                            var billeteraVendedor = await billeteraRepository.ObtenerPorUsuarioIdAsync(subasta.VendedorId, stoppingToken);

                            if (billeteraComprador is not null && billeteraVendedor is not null)
                            {
                                billeteraComprador.SaldoTotal -= pujaGanadora.Monto;
                                billeteraComprador.SaldoRetenido -= pujaGanadora.Monto;
                                billeteraVendedor.SaldoTotal += pujaGanadora.Monto;

                                billeteraRepository.AgregarMovimiento(new TransaccionLedger
                                {
                                    BilleteraId = billeteraComprador.Id,
                                    Tipo = TipoMovimiento.Pago,
                                    Monto = pujaGanadora.Monto,
                                    Fecha = ahora,
                                    SubastaId = subasta.Id
                                });

                                billeteraRepository.AgregarMovimiento(new TransaccionLedger
                                {
                                    BilleteraId = billeteraVendedor.Id,
                                    Tipo = TipoMovimiento.Cobro,
                                    Monto = pujaGanadora.Monto,
                                    Fecha = ahora,
                                    SubastaId = subasta.Id
                                });
                            }

                            subastaRepository.AgregarAuditoria(new AuditoriaLog
                            {
                                Entidad = "Subasta",
                                EntidadId = subasta.Id,
                                Accion = "CIERRE_CON_GANADOR",
                                UsuarioId = null,
                                DetalleJson = System.Text.Json.JsonSerializer.Serialize(new
                                {
                                    compradorId = pujaGanadora.CompradorId,
                                    monto = pujaGanadora.Monto
                                }),
                                Fecha = ahora
                            });
                        }

                        subastaRepository.ActualizarSubasta(subasta);
                        await unitOfWork.SaveChangesAsync(stoppingToken);
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        _logger.LogWarning(ex, "Conflicto de concurrencia al liquidar subasta {SubastaId}, se reintentará en el próximo ciclo", subasta.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error inesperado al liquidar subasta {SubastaId}", subasta.Id);
                    }
                }
            }

            await Task.Delay(Intervalo, stoppingToken);
        }
    }
}