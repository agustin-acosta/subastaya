using Application.Interfaces;
using Domain;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<Usuario> _hasher = new();

    public string Hashear(string passwordPlano)
    {
        return _hasher.HashPassword(new Usuario(), passwordPlano);
    }

    public bool Verificar(string hash, string passwordPlano)
    {
        var resultado = _hasher.VerifyHashedPassword(new Usuario(), hash, passwordPlano);
        return resultado == PasswordVerificationResult.Success;
    }
}