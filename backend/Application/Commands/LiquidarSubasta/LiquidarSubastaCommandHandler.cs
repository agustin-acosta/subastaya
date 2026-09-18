using Application.Interfaces;
using Domain;

namespace Application.Commands.LiquidarSubasta;

public class LiquidarSubastaCommandHandler
{
    private readonly ISubastaRepository _subastaRepository;
    private readonly IBilleteraRepository _billeteraRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificadorSubastas _notificadorSubastas;

    public LiquidarSubastaCommandHandler(
        ISubastaRepository subastaRepository,
        IBilleteraRepository billeteraRepository,
        IUnitOfWork unitOfWork,
        INotificadorSubastas notificadorSubastas)
    {
        _subastaRepository = subastaRepository;
        _billeteraRepository = billeteraRepository;
        _unitOfWork = unitOfWork;
        _notificadorSubastas = notificadorSubastas;
    }

    public async Task Handle(LiquidarSubastaCommand command, CancellationToken cancellationToken)
    {
        var subasta = await _subastaRepository.ObtenerPorIdAsync(command.SubastaId, cancellationToken);
        if (subasta is null)
        {
            return;
        }

        var ahora = DateTime.UtcNow;

        if (subasta.Estado != EstadoSubasta.Activa && subasta.Estado != EstadoSubasta.Programada)
        {
            return;
        }

        if (subasta.FechaFin > ahora)
        {
            return;
        }

        var pujaGanadora = await _subastaRepository.ObtenerPujaConMayorMontoAsync(subasta.Id, cancellationToken);

        if (pujaGanadora is null)
        {
            subasta.Estado = EstadoSubasta.Desierta;

            _subastaRepository.AgregarAuditoria(new AuditoriaLog
            {
                Entidad = "Subasta",
                EntidadId = subasta.Id,
                Accion = "CIERRE_DESIERTA",
                UsuarioId = null,
                DetalleJson = "{}",
                Fecha = ahora
            });
        }
        else
        {
            subasta.Estado = EstadoSubasta.Finalizada;

            var billeteraComprador = await _billeteraRepository.ObtenerPorUsuarioIdAsync(pujaGanadora.CompradorId, cancellationToken);
            var billeteraVendedor = await _billeteraRepository.ObtenerPorUsuarioIdAsync(subasta.VendedorId, cancellationToken);

            if (billeteraComprador is not null && billeteraVendedor is not null)
            {
                billeteraComprador.SaldoTotal -= pujaGanadora.Monto;
                billeteraComprador.SaldoRetenido -= pujaGanadora.Monto;
                billeteraVendedor.SaldoTotal += pujaGanadora.Monto;

                _billeteraRepository.AgregarMovimiento(new TransaccionLedger
                {
                    BilleteraId = billeteraComprador.Id,
                    Tipo = TipoMovimiento.Pago,
                    Monto = pujaGanadora.Monto,
                    Fecha = ahora,
                    SubastaId = subasta.Id
                });

                _billeteraRepository.AgregarMovimiento(new TransaccionLedger
                {
                    BilleteraId = billeteraVendedor.Id,
                    Tipo = TipoMovimiento.Cobro,
                    Monto = pujaGanadora.Monto,
                    Fecha = ahora,
                    SubastaId = subasta.Id
                });
            }

            _subastaRepository.AgregarAuditoria(new AuditoriaLog
            {
                Entidad = "Subasta",
                EntidadId = subasta.Id,
                Accion = "CIERRE_CON_GANADOR",
                UsuarioId = null,
                DetalleJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    compradorId = pujaGanadora.CompradorId,
                    monto = pujaGanadora.Monto
                }),
                Fecha = ahora
            });
        }

        _subastaRepository.ActualizarSubasta(subasta);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificadorSubastas.NotificarCambioEstadoAsync(subasta.Id, subasta.Estado.ToString(), cancellationToken);
    }
}