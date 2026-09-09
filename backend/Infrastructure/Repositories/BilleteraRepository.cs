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

    public async Task<Billetera?> ObtenerPorUsuarioIdAsync(int usuarioId)
    {
        return await _context.Billeteras
            .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);
    }

    public void AgregarMovimiento(TransaccionLedger movimiento)
    {
        _context.TransaccionesLedger.Add(movimiento);
    }
}