using Application.Commands.CrearSubasta;
using Application.Commands.EliminarSubasta;
using Application.Commands.ModificarSubasta;
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
    private readonly ModificarSubastaCommandHandler _modificarSubastaHandler;
    private readonly EliminarSubastaCommandHandler _eliminarSubastaHandler;

    public SubastasController(
        ListarSubastasQueryHandler listarSubastasHandler,
        ObtenerSubastaQueryHandler obtenerSubastaHandler,
        OfertarCommandHandler ofertarHandler,
        CrearSubastaCommandHandler crearSubastaHandler,
        ModificarSubastaCommandHandler modificarSubastaHandler,
        EliminarSubastaCommandHandler eliminarSubastaHandler)
    {
        _listarSubastasHandler = listarSubastasHandler;
        _obtenerSubastaHandler = obtenerSubastaHandler;
        _ofertarHandler = ofertarHandler;
        _crearSubastaHandler = crearSubastaHandler;
        _modificarSubastaHandler = modificarSubastaHandler;
        _eliminarSubastaHandler = eliminarSubastaHandler;
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] string? estado, [FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10, CancellationToken cancellationToken = default)
    {
        var resultado = await _listarSubastasHandler.Handle(new ListarSubastasQuery(estado, pagina, tamanoPagina), cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        var resultado = await _obtenerSubastaHandler.Handle(new ObtenerSubastaQuery(id), cancellationToken);
        if (resultado is null)
        {
            return NotFound();
        }
        return Ok(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearSubastaDto dto, CancellationToken cancellationToken)
    {
        var comando = new CrearSubastaCommand(
            dto.VendedorId, dto.CategoriaId, dto.Titulo, dto.Descripcion,
            dto.UrlImagen, dto.PrecioBase, dto.IncrementoMinimo,
            dto.FechaInicio, dto.FechaFin);

        var subastaId = await _crearSubastaHandler.Handle(comando, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = subastaId }, new { id = subastaId });
    }

    public class OfertarRequest
    {
        public int CompradorId { get; set; }
        public decimal Monto { get; set; }
    }

    [HttpPost("{id}/pujas")]
    public async Task<IActionResult> Ofertar(int id, [FromBody] OfertarRequest request, CancellationToken cancellationToken)
    {
        var pujaId = await _ofertarHandler.Handle(new OfertarCommand(id, request.CompradorId, request.Monto), cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id }, new { pujaId });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Modificar(int id, [FromBody] ModificarSubastaDto dto, CancellationToken cancellationToken)
    {
        var comando = new ModificarSubastaCommand(
            id, dto.Titulo, dto.Descripcion, dto.UrlImagen,
            dto.PrecioBase, dto.IncrementoMinimo, dto.FechaFin);

        await _modificarSubastaHandler.Handle(comando, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        await _eliminarSubastaHandler.Handle(new EliminarSubastaCommand(id), cancellationToken);
        return NoContent();
    }
}