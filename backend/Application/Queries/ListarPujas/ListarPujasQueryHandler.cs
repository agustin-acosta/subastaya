using Application.Dtos;
using Application.Interfaces;

namespace Application.Queries.ListarPujas;

public class ListarPujasQueryHandler
{
    private readonly ISubastaRepository _subastaRepository;

    public ListarPujasQueryHandler(ISubastaRepository subastaRepository)
    {
        _subastaRepository = subastaRepository;
    }

    public async Task<List<PujaListItemDto>> Handle(ListarPujasQuery query, CancellationToken cancellationToken)
    {
        var pujas = await _subastaRepository.ObtenerPujasPorSubastaAsync(query.SubastaId, cancellationToken);

        return pujas.Select(p => new PujaListItemDto
        {
            EsMia = query.UsuarioActualId.HasValue && p.CompradorId == query.UsuarioActualId.Value,
            Monto = p.Monto,
            CompradorSeudonimo = $"Postor #{p.CompradorId}",
            FechaPuja = p.FechaPuja
        }).ToList();
    }
}