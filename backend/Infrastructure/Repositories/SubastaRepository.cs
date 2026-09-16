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

    private IQueryable<Subasta> AplicarFiltros(
        IQueryable<Subasta> query, string? estado, int? categoriaId, decimal? precioMin, decimal? precioMax)
    {
        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoSubasta>(estado, true, out var estadoParseado))
        {
            query = query.Where(s => s.Estado == estadoParseado);
        }

        if (categoriaId.HasValue)
        {
            query = query.Where(s => s.CategoriaId == categoriaId.Value);
        }

        if (precioMin.HasValue)
        {
            query = query.Where(s => (s.PujaActualMonto ?? s.PrecioBase) >= precioMin.Value);
        }

        if (precioMax.HasValue)
        {
            query = query.Where(s => (s.PujaActualMonto ?? s.PrecioBase) <= precioMax.Value);
        }

        return query;
    }

    private IQueryable<Subasta> AplicarOrden(IQueryable<Subasta> query, string? ordenarPor)
    {
        return ordenarPor switch
        {
            "tiempoRestante" => query.OrderBy(s => s.FechaFin),
            "mayorPuja" => query.OrderByDescending(s => s.PujaActualMonto ?? s.PrecioBase),
            _ => query.OrderByDescending(s => s.Id)
        };
    }

    public async Task<List<Subasta>> ObtenerTodasAsync(
        string? estado, int? categoriaId, decimal? precioMin, decimal? precioMax, string? ordenarPor,
        int pagina, int tamanoPagina, CancellationToken cancellationToken)
    {
        var query = _context.Subastas.Include(s => s.Categoria).AsQueryable();
        query = AplicarFiltros(query, estado, categoriaId, precioMin, precioMax);
        query = AplicarOrden(query, ordenarPor);

        return await query
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> ContarAsync(string? estado, int? categoriaId, decimal? precioMin, decimal? precioMax, CancellationToken cancellationToken)
    {
        var query = _context.Subastas.AsQueryable();
        query = AplicarFiltros(query, estado, categoriaId, precioMin, precioMax);
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

    public async Task<List<Subasta>> ObtenerPorVendedorAsync(int vendedorId, CancellationToken cancellationToken)
    {
        return await _context.Subastas
            .Include(s => s.Categoria)
            .Where(s => s.VendedorId == vendedorId)
            .OrderByDescending(s => s.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Subasta>> ObtenerConPujaDeUsuarioAsync(int usuarioId, CancellationToken cancellationToken)
    {
        return await _context.Subastas
            .Include(s => s.Categoria)
            .Include(s => s.Pujas)
            .Where(s => s.Pujas.Any(p => p.CompradorId == usuarioId))
            .OrderByDescending(s => s.Id)
            .ToListAsync(cancellationToken);
    }

    public void Agregar(Subasta subasta) => _context.Subastas.Add(subasta);
    public void ActualizarSubasta(Subasta subasta) => _context.Subastas.Update(subasta);
    public void AgregarPuja(Puja puja) => _context.Pujas.Add(puja);
    public void AgregarAuditoria(AuditoriaLog log) => _context.AuditoriaLogs.Add(log);
    public void Eliminar(Subasta subasta) => _context.Subastas.Remove(subasta);
}