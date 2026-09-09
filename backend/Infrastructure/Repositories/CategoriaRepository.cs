using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly SubastaYaDbContext _context;

    public CategoriaRepository(SubastaYaDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExisteAsync(int categoriaId)
    {
        return await _context.Categorias.AnyAsync(c => c.Id == categoriaId);
    }
}