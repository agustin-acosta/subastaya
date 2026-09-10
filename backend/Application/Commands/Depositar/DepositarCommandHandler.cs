using Application.Interfaces;
using Domain;
using Domain.Exceptions;

namespace Application.Commands.Depositar;

public class DepositarCommandHandler
{
    private readonly IBilleteraRepository _billeteraRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DepositarCommandHandler(IBilleteraRepository billeteraRepository, IUnitOfWork unitOfWork)
    {
        _billeteraRepository = billeteraRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DepositarCommand command)
    {
        if (command.Monto <= 0)
        {
            throw new OperacionInvalidaException("El monto a depositar debe ser mayor a cero.");
        }

        var billetera = await _billeteraRepository.ObtenerPorUsuarioIdAsync(command.UsuarioId);
        if (billetera is null)
        {
            throw new KeyNotFoundException("El usuario no existe.");
        }

        billetera.SaldoTotal += command.Monto;

        _billeteraRepository.AgregarMovimiento(new TransaccionLedger
        {
            BilleteraId = billetera.Id,
            Tipo = TipoMovimiento.Deposito,
            Monto = command.Monto,
            Fecha = DateTime.UtcNow,
            SubastaId = null
        });

        await _unitOfWork.SaveChangesAsync();
    }
}