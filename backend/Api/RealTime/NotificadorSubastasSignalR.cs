using Api.Hubs;
using Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Api.RealTime;

public class NotificadorSubastasSignalR : INotificadorSubastas
{
    private readonly IHubContext<SubastasHub> _hubContext;
    private readonly ILogger<NotificadorSubastasSignalR> _logger;

    public NotificadorSubastasSignalR(IHubContext<SubastasHub> hubContext, ILogger<NotificadorSubastasSignalR> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public Task NotificarNuevaPujaAsync(int subastaId, NuevaPujaNotificacion notificacion, CancellationToken cancellationToken)
        => EnviarAsync(subastaId, "NuevaPuja", notificacion, cancellationToken);

    public Task NotificarExtensionAsync(int subastaId, DateTime nuevaFechaFin, CancellationToken cancellationToken)
        => EnviarAsync(subastaId, "ExtensionTiempo", new { subastaId, nuevaFechaFin }, cancellationToken);

    public Task NotificarCambioEstadoAsync(int subastaId, string nuevoEstado, CancellationToken cancellationToken)
        => EnviarAsync(subastaId, "CambioEstado", new { subastaId, nuevoEstado }, cancellationToken);

    private async Task EnviarAsync(int subastaId, string metodo, object payload, CancellationToken cancellationToken)
    {
        try
        {
            await _hubContext.Clients
                .Group(SubastasHub.ObtenerNombreDeGrupo(subastaId))
                .SendAsync(metodo, payload, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo enviar la notificación '{Metodo}' para la subasta {SubastaId}", metodo, subastaId);
        }
    }
}