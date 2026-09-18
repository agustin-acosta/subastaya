using Application.Interfaces;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly SubastaYaDbContext _context;

    public UsuarioRepository(SubastaYaDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExisteAsync(int usuarioId, CancellationToken cancellationToken)
    {
        return await _context.Usuarios.AnyAsync(u => u.Id == usuarioId, cancellationToken);
    }

    public async Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }
}