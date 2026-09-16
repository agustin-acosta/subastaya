using Application.Dtos;
using Application.Interfaces;

namespace Application.Queries.ListarMisPublicaciones;

public class ListarMisPublicacionesQueryHandler
{
    private readonly ISubastaRepository _subastaRepository;

    public ListarMisPublicacionesQueryHandler(ISubastaRepository subastaRepository)
    {
        _subastaRepository = subastaRepository;
    }

    public async Task<List<SubastaListItemDto>> Handle(ListarMisPublicacionesQuery query, CancellationToken cancellationToken)
    {
        var subastas = await _subastaRepository.ObtenerPorVendedorAsync(query.VendedorId, cancellationToken);

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