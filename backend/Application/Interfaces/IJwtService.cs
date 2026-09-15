using Domain;

namespace Application.Interfaces;

public interface IJwtService
{
    string Generar(Usuario usuario);
}