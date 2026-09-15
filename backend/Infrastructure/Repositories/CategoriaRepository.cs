using Application.Interfaces;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly SubastaYaDbContext _context;

    public CategoriaRepository(SubastaYaDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExisteAsync(int categoriaId, CancellationToken cancellationToken)
    {
        return await _context.Categorias.AnyAsync(c => c.Id == categoriaId, cancellationToken);
    }

    public async Task<List<Categoria>> ObtenerTodasAsync(CancellationToken cancellationToken)
    {
        return await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync(cancellationToken);
    }
}