using Domain;

namespace Application.Interfaces;

public interface IBilleteraRepository
{
    Task<Billetera?> ObtenerPorUsuarioIdAsync(int usuarioId);
    void AgregarMovimiento(TransaccionLedger movimiento);
}