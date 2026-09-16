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

    public async Task Handle(DepositarCommand command, CancellationToken cancellationToken)
    {
        if (command.Monto <= 0)
        {
            throw new OperacionInvalidaException("El monto a depositar debe ser mayor a cero.");
        }

        var billetera = await _billeteraRepository.ObtenerPorUsuarioIdAsync(command.UsuarioId, cancellationToken);
        if (billetera is null)
        {
            throw new EntidadNoEncontradaException("El usuario no existe.");
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

        _billeteraRepository.AgregarAuditoria(new AuditoriaLog
        {
            Entidad = "Billetera",
            EntidadId = billetera.Id,
            Accion = "ACREDITACION_MANUAL",
            UsuarioId = command.UsuarioId,
            DetalleJson = System.Text.Json.JsonSerializer.Serialize(new { monto = command.Monto }),
            Fecha = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}