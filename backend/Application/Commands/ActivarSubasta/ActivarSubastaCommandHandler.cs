using Application.Interfaces;
using Domain;

namespace Application.Commands.ActivarSubasta;

public class ActivarSubastaCommandHandler
{
    private readonly ISubastaRepository _subastaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificadorSubastas _notificadorSubastas;

    public ActivarSubastaCommandHandler(
        ISubastaRepository subastaRepository,
        IUnitOfWork unitOfWork,
        INotificadorSubastas notificadorSubastas)
    {
        _subastaRepository = subastaRepository;
        _unitOfWork = unitOfWork;
        _notificadorSubastas = notificadorSubastas;
    }

    public async Task Handle(ActivarSubastaCommand command, CancellationToken cancellationToken)
    {
        var subasta = await _subastaRepository.ObtenerPorIdAsync(command.SubastaId, cancellationToken);

        if (subasta is null || subasta.Estado != EstadoSubasta.Programada)
        {
            return;
        }

        subasta.Estado = EstadoSubasta.Activa;

        _subastaRepository.AgregarAuditoria(new AuditoriaLog
        {
            Entidad = "Subasta",
            EntidadId = subasta.Id,
            Accion = "ACTIVACION",
            UsuarioId = null,
            DetalleJson = "{}",
            Fecha = DateTime.UtcNow
        });

        _subastaRepository.ActualizarSubasta(subasta);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificadorSubastas.NotificarCambioEstadoAsync(subasta.Id, subasta.Estado.ToString(), cancellationToken);
    }
}