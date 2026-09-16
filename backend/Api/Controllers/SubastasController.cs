using System.Security.Claims;
using Application.Commands.CrearSubasta;
using Application.Commands.EliminarSubasta;
using Application.Commands.ModificarSubasta;
using Application.Commands.Ofertar;
using Application.Dtos;
using Application.Queries.ListarPujas;
using Application.Queries.ListarSubastas;
using Application.Queries.ObtenerSubasta;
using Microsoft.AspNetCore.Authorization;
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
    private readonly ListarPujasQueryHandler _listarPujasHandler;

    public SubastasController(
        ListarSubastasQueryHandler listarSubastasHandler,
        ObtenerSubastaQueryHandler obtenerSubastaHandler,
        OfertarCommandHandler ofertarHandler,
        CrearSubastaCommandHandler crearSubastaHandler,
        ModificarSubastaCommandHandler modificarSubastaHandler,
        EliminarSubastaCommandHandler eliminarSubastaHandler,
        ListarPujasQueryHandler listarPujasHandler)
    {
        _listarSubastasHandler = listarSubastasHandler;
        _obtenerSubastaHandler = obtenerSubastaHandler;
        _ofertarHandler = ofertarHandler;
        _crearSubastaHandler = crearSubastaHandler;
        _modificarSubastaHandler = modificarSubastaHandler;
        _eliminarSubastaHandler = eliminarSubastaHandler;
        _listarPujasHandler = listarPujasHandler;
    }

    private int UsuarioActualId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? estado,
        [FromQuery] int? categoriaId,
        [FromQuery] decimal? precioMin,
        [FromQuery] decimal? precioMax,
        [FromQuery] string? ordenarPor,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10,
        CancellationToken cancellationToken = default)
    {
        var resultado = await _listarSubastasHandler.Handle(
            new ListarSubastasQuery(estado, categoriaId, precioMin, precioMax, ordenarPor, pagina, tamanoPagina),
            cancellationToken);
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

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearSubastaDto dto, CancellationToken cancellationToken)
    {
        var comando = new CrearSubastaCommand(
            UsuarioActualId, dto.CategoriaId, dto.Titulo, dto.Descripcion,
            dto.UrlImagen, dto.PrecioBase, dto.IncrementoMinimo,
            dto.FechaInicio, dto.FechaFin);

        var subastaId = await _crearSubastaHandler.Handle(comando, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = subastaId }, new { id = subastaId });
    }

    public class OfertarRequest
    {
        public decimal Monto { get; set; }
    }

    [Authorize]
    [HttpPost("{id}/pujas")]
    public async Task<IActionResult> Ofertar(int id, [FromBody] OfertarRequest request, CancellationToken cancellationToken)
    {
        var pujaId = await _ofertarHandler.Handle(new OfertarCommand(id, UsuarioActualId, request.Monto), cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id }, new { pujaId });
    }

    [HttpGet("{id}/pujas")]
    public async Task<IActionResult> ListarPujas(int id, CancellationToken cancellationToken)
    {
        var resultado = await _listarPujasHandler.Handle(new ListarPujasQuery(id), cancellationToken);
        return Ok(resultado);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Modificar(int id, [FromBody] ModificarSubastaDto dto, CancellationToken cancellationToken)
    {
        var comando = new ModificarSubastaCommand(
            id, UsuarioActualId, dto.Titulo, dto.Descripcion, dto.UrlImagen,
            dto.PrecioBase, dto.IncrementoMinimo, dto.FechaFin);

        await _modificarSubastaHandler.Handle(comando, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        await _eliminarSubastaHandler.Handle(new EliminarSubastaCommand(id, UsuarioActualId), cancellationToken);
        return NoContent();
    }
}