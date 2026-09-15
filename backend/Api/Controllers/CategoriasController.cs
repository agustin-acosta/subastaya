using Application.Queries.ListarCategorias;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/categorias")]
public class CategoriasController : ControllerBase
{
    private readonly ListarCategoriasQueryHandler _handler;

    public CategoriasController(ListarCategoriasQueryHandler handler)
    {
        _handler = handler;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var resultado = await _handler.Handle(new ListarCategoriasQuery(), cancellationToken);
        return Ok(resultado);
    }
}