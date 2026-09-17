namespace Application.Interfaces;

public interface INotificadorSubastas
{
    Task NotificarNuevaPujaAsync(int subastaId, NuevaPujaNotificacion notificacion, CancellationToken cancellationToken);
    Task NotificarExtensionAsync(int subastaId, DateTime nuevaFechaFin, CancellationToken cancellationToken);
    Task NotificarCambioEstadoAsync(int subastaId, string nuevoEstado, CancellationToken cancellationToken);
}
public record NuevaPujaNotificacion(int CompradorId, string CompradorSeudonimo, decimal Monto, DateTime FechaPuja);