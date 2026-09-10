using Application.Interfaces;
using Domain.Exceptions;

namespace Application.Commands.EliminarSubasta;

public class EliminarSubastaCommandHandler
{
    private readonly ISubastaRepository _subastaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EliminarSubastaCommandHandler(ISubastaRepository subastaRepository, IUnitOfWork unitOfWork)
    {
        _subastaRepository = subastaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(EliminarSubastaCommand command)
    {
        var subasta = await _subastaRepository.ObtenerPorIdAsync(command.SubastaId);
        if (subasta is null)
        {
            throw new KeyNotFoundException("La subasta no existe.");
        }

        if (subasta.Pujas.Count > 0)
        {
            throw new OperacionInvalidaException("No se puede eliminar una subasta que ya tiene ofertas.");
        }

        _subastaRepository.Eliminar(subasta);
        await _unitOfWork.SaveChangesAsync();
    }
}