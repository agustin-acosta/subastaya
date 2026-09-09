namespace Application.Interfaces;

public interface IUsuarioRepository
{
    Task<bool> ExisteAsync(int usuarioId);
}