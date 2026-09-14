using Domain;

namespace Application.Interfaces;

public interface IBilleteraRepository
{
    Task<Billetera?> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken);
    void AgregarMovimiento(TransaccionLedger movimiento);
}