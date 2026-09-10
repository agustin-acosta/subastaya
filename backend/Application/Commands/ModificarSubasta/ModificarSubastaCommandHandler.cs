using Application.Interfaces;
using Domain.Exceptions;

namespace Application.Commands.ModificarSubasta;

public class ModificarSubastaCommandHandler
{
    private readonly ISubastaRepository _subastaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ModificarSubastaCommandHandler(ISubastaRepository subastaRepository, IUnitOfWork unitOfWork)
    {
        _subastaRepository = subastaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ModificarSubastaCommand command)
    {
        var subasta = await _subastaRepository.ObtenerPorIdAsync(command.SubastaId);
        if (subasta is null)
        {
            throw new KeyNotFoundException("La subasta no existe.");
        }

        if (subasta.Pujas.Count > 0)
        {
            throw new OperacionInvalidaException("No se puede modificar una subasta que ya tiene ofertas.");
        }

        if (command.FechaFin <= subasta.FechaInicio)
        {
            throw new OperacionInvalidaException("La fecha de fin debe ser posterior a la fecha de inicio.");
        }

        if (command.PrecioBase <= 0 || command.IncrementoMinimo <= 0)
        {
            throw new OperacionInvalidaException("El precio base y el incremento mínimo deben ser mayores a cero.");
        }

        subasta.Titulo = command.Titulo;
        subasta.Descripcion = command.Descripcion;
        subasta.UrlImagen = command.UrlImagen;
        subasta.PrecioBase = command.PrecioBase;
        subasta.IncrementoMinimo = command.IncrementoMinimo;
        subasta.FechaFin = command.FechaFin;

        await _unitOfWork.SaveChangesAsync();
    }
}