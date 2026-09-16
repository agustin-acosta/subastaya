using Application.Dtos;
using Application.Interfaces;

namespace Application.Queries.ListarMisPujas;

public class ListarMisPujasQueryHandler
{
    private readonly ISubastaRepository _subastaRepository;

    public ListarMisPujasQueryHandler(ISubastaRepository subastaRepository)
    {
        _subastaRepository = subastaRepository;
    }

    public async Task<List<MiPujaDto>> Handle(ListarMisPujasQuery query, CancellationToken cancellationToken)
    {
        var subastas = await _subastaRepository.ObtenerConPujaDeUsuarioAsync(query.UsuarioId, cancellationToken);

        return subastas
            .Select(s =>
            {
                var miMejorOferta = s.Pujas.Where(p => p.CompradorId == query.UsuarioId).Max(p => p.Monto);
                var precioActual = s.PujaActualMonto ?? s.PrecioBase;

                return new MiPujaDto
                {
                    SubastaId = s.Id,
                    Titulo = s.Titulo,
                    UrlImagen = s.UrlImagen,
                    Estado = s.Estado.ToString(),
                    FechaFin = s.FechaFin,
                    PrecioActual = precioActual,
                    MiMejorOferta = miMejorOferta,
                    EstoyLiderando = miMejorOferta == precioActual
                };
            })
            .OrderByDescending(d => d.SubastaId)
            .ToList();
    }
}