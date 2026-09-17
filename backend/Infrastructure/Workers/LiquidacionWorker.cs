using Application.Commands.ActivarSubasta;
using Application.Commands.LiquidarSubasta;
using Application.Interfaces;
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
            await ActivarProgramadasAsync(stoppingToken);
            await LiquidarVencidasAsync(stoppingToken);

            await Task.Delay(Intervalo, stoppingToken);
        }
    }

    private async Task ActivarProgramadasAsync(CancellationToken stoppingToken)
    {
        List<int> idsParaActivar;

        using (var scopeConsulta = _scopeFactory.CreateScope())
        {
            var subastaRepository = scopeConsulta.ServiceProvider.GetRequiredService<ISubastaRepository>();
            var ahora = DateTime.UtcNow;

            try
            {
                var programadas = await subastaRepository.ObtenerProgramadasParaActivarAsync(ahora, stoppingToken);
                idsParaActivar = programadas.Select(s => s.Id).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar subastas programadas para activar");
                idsParaActivar = new List<int>();
            }
        }

        foreach (var subastaId in idsParaActivar)
        {
            using var scope = _scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<ActivarSubastaCommandHandler>();

            try
            {
                await handler.Handle(new ActivarSubastaCommand(subastaId), stoppingToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Conflicto de concurrencia al activar subasta {SubastaId}, se reintentará en el próximo ciclo", subastaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al activar subasta {SubastaId}", subastaId);
            }
        }
    }

    private async Task LiquidarVencidasAsync(CancellationToken stoppingToken)
    {
        List<int> idsVencidas;

        using (var scopeConsulta = _scopeFactory.CreateScope())
        {
            var subastaRepository = scopeConsulta.ServiceProvider.GetRequiredService<ISubastaRepository>();
            var ahora = DateTime.UtcNow;

            try
            {
                var vencidas = await subastaRepository.ObtenerVencidasSinLiquidarAsync(ahora, stoppingToken);
                idsVencidas = vencidas.Select(s => s.Id).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar subastas vencidas");
                idsVencidas = new List<int>();
            }
        }

        foreach (var subastaId in idsVencidas)
        {
            using var scope = _scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<LiquidarSubastaCommandHandler>();

            try
            {
                await handler.Handle(new LiquidarSubastaCommand(subastaId), stoppingToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Conflicto de concurrencia al liquidar subasta {SubastaId}, se reintentará en el próximo ciclo", subastaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al liquidar subasta {SubastaId}", subastaId);
            }
        }
    }
}