using Domain;

namespace Application.Interfaces;

public interface ICategoriaRepository
{
    Task<bool> ExisteAsync(int categoriaId, CancellationToken cancellationToken);
    Task<List<Categoria>> ObtenerTodasAsync(CancellationToken cancellationToken);
}