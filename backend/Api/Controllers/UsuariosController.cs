using Application.Queries.ListarUsuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly ListarUsuariosQueryHandler _listarUsuariosHandler;

    public UsuariosController(ListarUsuariosQueryHandler listarUsuariosHandler)
    {
        _listarUsuariosHandler = listarUsuariosHandler;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var resultado = await _listarUsuariosHandler.Handle(new ListarUsuariosQuery(), cancellationToken);
        return Ok(resultado);
    }
}