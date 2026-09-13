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

    private IQueryable<Subasta> AplicarFiltroEstado(IQueryable<Subasta> query, string? estado)
    {
        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoSubasta>(estado, true, out var estadoParseado))
        {
            query = query.Where(s => s.Estado == estadoParseado);
        }
        return query;
    }

    public async Task<List<Subasta>> ObtenerTodasAsync(string? estado, int pagina, int tamanoPagina)
    {
        var query = _context.Subastas.Include(s => s.Categoria).AsQueryable();
        query = AplicarFiltroEstado(query, estado);

        return await query
            .OrderByDescending(s => s.Id)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync();
    }

    public async Task<int> ContarAsync(string? estado)
    {
        var query = _context.Subastas.AsQueryable();
        query = AplicarFiltroEstado(query, estado);
        return await query.CountAsync();
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

    public async Task<List<Subasta>> ObtenerVencidasSinLiquidarAsync(DateTime ahora)
    {
        return await _context.Subastas
            .Where(s => s.FechaFin < ahora &&
                        (s.Estado == EstadoSubasta.Activa || s.Estado == EstadoSubasta.Programada))
            .ToListAsync();
    }

    public async Task<Puja?> ObtenerPujaGanadoraAsync(int subastaId)
    {
        return await _context.Pujas
            .Where(p => p.SubastaId == subastaId)
            .OrderByDescending(p => p.Monto)
            .FirstOrDefaultAsync();
    }

<<<<<<< HEAD
    public void Agregar(Subasta subasta) => _context.Subastas.Add(subasta);
    public void ActualizarSubasta(Subasta subasta) => _context.Subastas.Update(subasta);
    public void AgregarPuja(Puja puja) => _context.Pujas.Add(puja);
    public void AgregarAuditoria(AuditoriaLog log) => _context.AuditoriaLogs.Add(log);
    public void Eliminar(Subasta subasta) => _context.Subastas.Remove(subasta);
=======
    public void AgregarAuditoria(AuditoriaLog log)
    {
        _context.AuditoriaLogs.Add(log);
    }
    public void Agregar(Subasta subasta)
    {
        _context.Subastas.Add(subasta);
    }
    public async Task<List<Subasta>> ObtenerVencidasSinLiquidarAsync(DateTime ahora)
    {
        return await _context.Subastas
            .Where(s => s.FechaFin < ahora &&
                        (s.Estado == EstadoSubasta.Activa || s.Estado == EstadoSubasta.Programada))
            .ToListAsync();
    }
    public async Task<Puja?> ObtenerPujaGanadoraAsync(int subastaId)
    {
        return await _context.Pujas
            .Where(p => p.SubastaId == subastaId)
            .OrderByDescending(p => p.Monto)
            .FirstOrDefaultAsync();
    }
    public void Eliminar(Subasta subasta)
    {
        _context.Subastas.Remove(subasta);
    }
>>>>>>> d7b8f7fad8cac88cfd89b395df862ee7a219c6a7
}