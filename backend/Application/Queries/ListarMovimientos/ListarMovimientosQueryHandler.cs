using Application.Dtos;
using Application.Interfaces;

namespace Application.Queries.ListarMovimientos;

public class ListarMovimientosQueryHandler
{
    private readonly IBilleteraRepository _billeteraRepository;

    public ListarMovimientosQueryHandler(IBilleteraRepository billeteraRepository)
    {
        _billeteraRepository = billeteraRepository;
    }

    public async Task<List<MovimientoDto>> Handle(ListarMovimientosQuery query, CancellationToken cancellationToken)
    {
        var billetera = await _billeteraRepository.ObtenerPorUsuarioIdAsync(query.UsuarioId, cancellationToken);
        if (billetera is null)
        {
            return new List<MovimientoDto>();
        }

        var movimientos = await _billeteraRepository.ObtenerMovimientosAsync(billetera.Id, cancellationToken);

        return movimientos.Select(m => new MovimientoDto
        {
            Tipo = m.Tipo.ToString(),
            Monto = m.Monto,
            Fecha = m.Fecha,
            SubastaId = m.SubastaId
        }).ToList();
    }
}