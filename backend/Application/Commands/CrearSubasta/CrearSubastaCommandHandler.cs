using Application.Interfaces;
using Domain;
using Domain.Exceptions;

namespace Application.Commands.CrearSubasta;

public class CrearSubastaCommandHandler
{
    private readonly ISubastaRepository _subastaRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CrearSubastaCommandHandler(
        ISubastaRepository subastaRepository,
        IUsuarioRepository usuarioRepository,
        ICategoriaRepository categoriaRepository,
        IUnitOfWork unitOfWork)
    {
        _subastaRepository = subastaRepository;
        _usuarioRepository = usuarioRepository;
        _categoriaRepository = categoriaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CrearSubastaCommand command)
    {
        if (!await _usuarioRepository.ExisteAsync(command.VendedorId))
        {
            throw new KeyNotFoundException("El vendedor no existe.");
        }

        if (!await _categoriaRepository.ExisteAsync(command.CategoriaId))
        {
            throw new KeyNotFoundException("La categoría no existe.");
        }

        if (command.FechaFin <= command.FechaInicio)
        {
            throw new OperacionInvalidaException("La fecha de fin debe ser posterior a la fecha de inicio.");
        }

        if (command.PrecioBase <= 0 || command.IncrementoMinimo <= 0)
        {
            throw new OperacionInvalidaException("El precio base y el incremento mínimo deben ser mayores a cero.");
        }

        var ahora = DateTime.UtcNow;
        var estadoInicial = command.FechaInicio > ahora ? EstadoSubasta.Programada : EstadoSubasta.Activa;

        var subasta = new Subasta
        {
            VendedorId = command.VendedorId,
            CategoriaId = command.CategoriaId,
            Titulo = command.Titulo,
            Descripcion = command.Descripcion,
            UrlImagen = command.UrlImagen,
            PrecioBase = command.PrecioBase,
            IncrementoMinimo = command.IncrementoMinimo,
            FechaInicio = command.FechaInicio,
            FechaFin = command.FechaFin,
            Estado = estadoInicial,
            PujaActualMonto = null
        };

        _subastaRepository.Agregar(subasta);
        await _unitOfWork.SaveChangesAsync();

        return subasta.Id;
    }
}