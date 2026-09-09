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

    public async Task<Puja?> ObtenerUltimaPujaAsync(int subastaId)
    {
        return await _context.Pujas
            .Where(p => p.SubastaId == subastaId)
            .OrderByDescending(p => p.Monto)
            .FirstOrDefaultAsync();
    }

    public void ActualizarSubasta(Subasta subasta)
    {
        _context.Subastas.Update(subasta);
    }

    public void AgregarPuja(Puja puja)
    {
        _context.Pujas.Add(puja);
    }

    public void AgregarAuditoria(AuditoriaLog log)
    {
        _context.AuditoriaLogs.Add(log);
    }
    public void Agregar(Subasta subasta)
    {
        _context.Subastas.Add(subasta);
    }
}