using Application.Commands.CrearSubasta;
using Application.Commands.Ofertar;
using Application.Dtos;
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
    private readonly CrearSubastaCommandHandler _crearSubastaHandler;

    public SubastasController(
        ListarSubastasQueryHandler listarSubastasHandler,
        ObtenerSubastaQueryHandler obtenerSubastaHandler,
        OfertarCommandHandler ofertarHandler,
        CrearSubastaCommandHandler crearSubastaHandler)
    {
        _listarSubastasHandler = listarSubastasHandler;
        _obtenerSubastaHandler = obtenerSubastaHandler;
        _ofertarHandler = ofertarHandler;
        _crearSubastaHandler = crearSubastaHandler;
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

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearSubastaDto dto)
    {
        var comando = new CrearSubastaCommand(
            dto.VendedorId, dto.CategoriaId, dto.Titulo, dto.Descripcion,
            dto.UrlImagen, dto.PrecioBase, dto.IncrementoMinimo,
            dto.FechaInicio, dto.FechaFin);

        var subastaId = await _crearSubastaHandler.Handle(comando);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = subastaId }, new { id = subastaId });
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