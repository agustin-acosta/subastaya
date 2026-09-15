namespace Application.Interfaces;

public interface IPasswordHasher
{
    string Hashear(string passwordPlano);
    bool Verificar(string hash, string passwordPlano);
}