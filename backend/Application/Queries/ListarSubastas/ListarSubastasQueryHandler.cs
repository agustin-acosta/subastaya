using Application.Dtos;
using Application.Interfaces;

namespace Application.Queries.ListarSubastas;

public class ListarSubastasQueryHandler
{
    private readonly ISubastaRepository _subastaRepository;

    public ListarSubastasQueryHandler(ISubastaRepository subastaRepository)
    {
        _subastaRepository = subastaRepository;
    }

    public async Task<List<SubastaListItemDto>> Handle(ListarSubastasQuery query)
    {
        var subastas = await _subastaRepository.ObtenerTodasAsync(query.Estado);

        return subastas.Select(s => new SubastaListItemDto
        {
            Id = s.Id,
            Titulo = s.Titulo,
            UrlImagen = s.UrlImagen,
            CategoriaNombre = s.Categoria.Nombre,
            PujaActualMonto = s.PujaActualMonto,
            PrecioBase = s.PrecioBase,
            FechaFin = s.FechaFin,
            Estado = s.Estado.ToString()
        }).ToList();
    }
}