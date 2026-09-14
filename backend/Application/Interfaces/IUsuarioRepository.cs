using Domain;

namespace Application.Interfaces;

public interface IUsuarioRepository
{
    Task<bool> ExisteAsync(int usuarioId, CancellationToken cancellationToken);
    Task<List<Usuario>> ObtenerTodosAsync(CancellationToken cancellationToken);
}