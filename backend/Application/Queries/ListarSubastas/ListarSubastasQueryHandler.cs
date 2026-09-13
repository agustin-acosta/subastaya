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

    public async Task<PaginacionDto<SubastaListItemDto>> Handle(ListarSubastasQuery query)
    {
        var subastas = await _subastaRepository.ObtenerTodasAsync(query.Estado, query.Pagina, query.TamanoPagina);
        var total = await _subastaRepository.ContarAsync(query.Estado);

        var items = subastas.Select(s => new SubastaListItemDto
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

        return new PaginacionDto<SubastaListItemDto>
        {
            Items = items,
            PaginaActual = query.Pagina,
            TamanoPagina = query.TamanoPagina,
            TotalItems = total,
            TotalPaginas = (int)Math.Ceiling(total / (double)query.TamanoPagina)
        };
    }
}