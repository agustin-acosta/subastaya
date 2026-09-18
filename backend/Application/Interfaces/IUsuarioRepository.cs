using Domain;

namespace Application.Interfaces;

public interface IUsuarioRepository
{
    Task<bool> ExisteAsync(int usuarioId, CancellationToken cancellationToken);
    Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken);
}