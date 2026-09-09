using Application.Commands.Ofertar;
using Application.Queries.ListarSubastas;
using Application.Queries.ObtenerSubasta;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/subastas")]
public class SubastasController : ControllerBase
{
    private readonly ListarSubastasQueryHandler _listarSubastasHandler;
    private readonly ObtenerSubastaQueryHandler _obtenerSubastaHandler;
    private readonly OfertarCommandHandler _ofertarHandler;

    public SubastasController(
        ListarSubastasQueryHandler listarSubastasHandler,
        ObtenerSubastaQueryHandler obtenerSubastaHandler,
        OfertarCommandHandler ofertarHandler)
    {
        _listarSubastasHandler = listarSubastasHandler;
        _obtenerSubastaHandler = obtenerSubastaHandler;
        _ofertarHandler = ofertarHandler;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var resultado = await _listarSubastasHandler.Handle(new ListarSubastasQuery());
        return Ok(resultado);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var resultado = await _obtenerSubastaHandler.Handle(new ObtenerSubastaQuery(id));
        if (resultado is null)
        {
            return NotFound();
        }
        return Ok(resultado);
    }

    public class OfertarRequest
    {
        public int CompradorId { get; set; }
        public decimal Monto { get; set; }
    }

    [HttpPost("{id}/pujas")]
    public async Task<IActionResult> Ofertar(int id, [FromBody] OfertarRequest request)
    {
        var pujaId = await _ofertarHandler.Handle(new OfertarCommand(id, request.CompradorId, request.Monto));
        return CreatedAtAction(nameof(ObtenerPorId), new { id }, new { pujaId });
    }
}