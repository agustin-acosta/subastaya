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

    public SubastasController(
        ListarSubastasQueryHandler listarSubastasHandler,
        ObtenerSubastaQueryHandler obtenerSubastaHandler)
    {
        _listarSubastasHandler = listarSubastasHandler;
        _obtenerSubastaHandler = obtenerSubastaHandler;
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
}