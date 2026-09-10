using Application.Interfaces;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Workers;

public class LiquidacionWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private static readonly TimeSpan Intervalo = TimeSpan.FromSeconds(30);

    public LiquidacionWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
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
                var vencidas = await subastaRepository.ObtenerVencidasSinLiquidarAsync(ahora);

                foreach (var subasta in vencidas)
                {
                    var pujaGanadora = await subastaRepository.ObtenerPujaGanadoraAsync(subasta.Id);

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

                        var billeteraComprador = await billeteraRepository.ObtenerPorUsuarioIdAsync(pujaGanadora.CompradorId);
                        var billeteraVendedor = await billeteraRepository.ObtenerPorUsuarioIdAsync(subasta.VendedorId);

                        if (billeteraComprador is not null && billeteraVendedor is not null)
                        {
                            // el comprador ya tenia el monto retenido, ahora se lo debita del total.
                            billeteraComprador.SaldoTotal -= pujaGanadora.Monto;
                            billeteraComprador.SaldoRetenido -= pujaGanadora.Monto;

                            // el vendedor recibe el monto.
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
                            DetalleJson = $"{{\"compradorId\":{pujaGanadora.CompradorId},\"monto\":{pujaGanadora.Monto}}}",
                            Fecha = ahora
                        });
                    }

                    subastaRepository.ActualizarSubasta(subasta);
                }

                if (vencidas.Count > 0)
                {
                    await unitOfWork.SaveChangesAsync();
                }
            }

            await Task.Delay(Intervalo, stoppingToken);
        }
    }
}