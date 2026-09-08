using Application.Dtos;
using Application.Interfaces;

namespace Application.Queries.ObtenerSubasta;

public class ObtenerSubastaQueryHandler
{
    private readonly ISubastaRepository _subastaRepository;

    public ObtenerSubastaQueryHandler(ISubastaRepository subastaRepository)
    {
        _subastaRepository = subastaRepository;
    }

    public async Task<SubastaDetalleDto?> Handle(ObtenerSubastaQuery query)
    {
        var subasta = await _subastaRepository.ObtenerPorIdAsync(query.Id);

        if (subasta is null)
        {
            return null;
        }

        return new SubastaDetalleDto
        {
            Id = subasta.Id,
            Titulo = subasta.Titulo,
            Descripcion = subasta.Descripcion,
            UrlImagen = subasta.UrlImagen,
            CategoriaNombre = subasta.Categoria.Nombre,
            VendedorNombre = subasta.Vendedor.Nombre,
            PrecioBase = subasta.PrecioBase,
            IncrementoMinimo = subasta.IncrementoMinimo,
            PujaActualMonto = subasta.PujaActualMonto,
            FechaInicio = subasta.FechaInicio,
            FechaFin = subasta.FechaFin,
            Estado = subasta.Estado.ToString(),
            CantidadPujas = subasta.Pujas.Count
        };
    }
}