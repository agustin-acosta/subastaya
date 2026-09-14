using Application.Dtos;
using Application.Interfaces;

namespace Application.Queries.ObtenerBalance;

public class ObtenerBalanceQueryHandler
{
    private readonly IBilleteraRepository _billeteraRepository;

    public ObtenerBalanceQueryHandler(IBilleteraRepository billeteraRepository)
    {
        _billeteraRepository = billeteraRepository;
    }

    public async Task<BalanceDto?> Handle(ObtenerBalanceQuery query, CancellationToken cancellationToken)
    {
        var billetera = await _billeteraRepository.ObtenerPorUsuarioIdAsync(query.UsuarioId, cancellationToken);

        if (billetera is null)
        {
            return null;
        }

        return new BalanceDto
        {
            SaldoTotal = billetera.SaldoTotal,
            SaldoRetenido = billetera.SaldoRetenido,
            SaldoDisponible = billetera.SaldoDisponible
        };
    }
}