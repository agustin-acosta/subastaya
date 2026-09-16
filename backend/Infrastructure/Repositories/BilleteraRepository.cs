using Application.Interfaces;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class BilleteraRepository : IBilleteraRepository
{
    private readonly SubastaYaDbContext _context;

    public BilleteraRepository(SubastaYaDbContext context)
    {
        _context = context;
    }

    public async Task<Billetera?> ObtenerPorUsuarioIdAsync(int usuarioId, CancellationToken cancellationToken)
    {
        return await _context.Billeteras
            .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId, cancellationToken);
    }

    public async Task<List<TransaccionLedger>> ObtenerMovimientosAsync(int billeteraId, CancellationToken cancellationToken)
    {
        return await _context.TransaccionesLedger
            .Where(t => t.BilleteraId == billeteraId)
            .OrderByDescending(t => t.Fecha)
            .ToListAsync(cancellationToken);
    }

    public void AgregarMovimiento(TransaccionLedger movimiento)
    {
        _context.TransaccionesLedger.Add(movimiento);
    }

    public void AgregarAuditoria(AuditoriaLog log)
    {
        _context.AuditoriaLogs.Add(log);
    }
}