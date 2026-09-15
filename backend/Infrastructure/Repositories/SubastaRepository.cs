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

    public async Task<List<Subasta>> ObtenerTodasAsync(string? estado, int pagina, int tamanoPagina, CancellationToken cancellationToken)
    {
        var query = _context.Subastas.Include(s => s.Categoria).AsQueryable();
        query = AplicarFiltroEstado(query, estado);

        return await query
            .OrderByDescending(s => s.Id)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> ContarAsync(string? estado, CancellationToken cancellationToken)
    {
        var query = _context.Subastas.AsQueryable();
        query = AplicarFiltroEstado(query, estado);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<Subasta?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Subastas
            .Include(s => s.Categoria)
            .Include(s => s.Vendedor)
            .Include(s => s.Pujas)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Puja?> ObtenerPujaConMayorMontoAsync(int subastaId, CancellationToken cancellationToken)
    {
        return await _context.Pujas
            .Where(p => p.SubastaId == subastaId)
            .OrderByDescending(p => p.Monto)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Puja>> ObtenerPujasPorSubastaAsync(int subastaId, CancellationToken cancellationToken)
    {
        return await _context.Pujas
            .Where(p => p.SubastaId == subastaId)
            .OrderByDescending(p => p.FechaPuja)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Subasta>> ObtenerVencidasSinLiquidarAsync(DateTime ahora, CancellationToken cancellationToken)
    {
        return await _context.Subastas
            .Where(s => s.FechaFin < ahora &&
                        (s.Estado == EstadoSubasta.Activa || s.Estado == EstadoSubasta.Programada))
            .ToListAsync(cancellationToken);
    }

    public void Agregar(Subasta subasta) => _context.Subastas.Add(subasta);
    public void ActualizarSubasta(Subasta subasta) => _context.Subastas.Update(subasta);
    public void AgregarPuja(Puja puja) => _context.Pujas.Add(puja);
    public void AgregarAuditoria(AuditoriaLog log) => _context.AuditoriaLogs.Add(log);
    public void Eliminar(Subasta subasta) => _context.Subastas.Remove(subasta);
}