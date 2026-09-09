using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly SubastaYaDbContext _context;

    public UsuarioRepository(SubastaYaDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExisteAsync(int usuarioId)
    {
        return await _context.Usuarios.AnyAsync(u => u.Id == usuarioId);
    }
}