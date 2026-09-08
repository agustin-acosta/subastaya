using Application.Interfaces;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class SubastaRepository : ISubastaRepository
{
    private readonly SubastaYaDbContext _context;

    public SubastaRepository(SubastaYaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Subasta>> ObtenerTodasAsync()
    {
        return await _context.Subastas
            .Include(s => s.Categoria)
            .ToListAsync();
    }

    public async Task<Subasta?> ObtenerPorIdAsync(int id)
    {
        return await _context.Subastas
            .Include(s => s.Categoria)
            .Include(s => s.Vendedor)
            .Include(s => s.Pujas)
            .FirstOrDefaultAsync(s => s.Id == id);
    }
}