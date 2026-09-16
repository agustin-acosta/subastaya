using Domain;

namespace Application.Interfaces;

public interface IBilleteraRepository
{
    Task<Billetera?> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken);
    Task<List<TransaccionLedger>> ObtenerMovimientosAsync(int billeteraId, CancellationToken cancellationToken);
    void AgregarMovimiento(TransaccionLedger movimiento);
}