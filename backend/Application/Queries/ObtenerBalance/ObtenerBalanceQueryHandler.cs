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

    public async Task<BalanceDto?> Handle(ObtenerBalanceQuery query)
    {
        var billetera = await _billeteraRepository.ObtenerPorUsuarioIdAsync(query.UsuarioId);

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