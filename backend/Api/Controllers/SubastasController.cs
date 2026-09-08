using Application.Queries.ListarSubastas;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/subastas")]
public class SubastasController : ControllerBase
{
    private readonly ListarSubastasQueryHandler _listarSubastasHandler;

    public SubastasController(ListarSubastasQueryHandler listarSubastasHandler)
    {
        _listarSubastasHandler = listarSubastasHandler;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var resultado = await _listarSubastasHandler.Handle(new ListarSubastasQuery());
        return Ok(resultado);
    }
}