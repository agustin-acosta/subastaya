namespace Application.Interfaces;

public interface ICategoriaRepository
{
    Task<bool> ExisteAsync(int categoriaId);
}