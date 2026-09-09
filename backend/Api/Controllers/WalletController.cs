using Application.Queries.ObtenerBalance;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/wallet")]
public class WalletController : ControllerBase
{
    private readonly ObtenerBalanceQueryHandler _obtenerBalanceHandler;

    public WalletController(ObtenerBalanceQueryHandler obtenerBalanceHandler)
    {
        _obtenerBalanceHandler = obtenerBalanceHandler;
    }

    [HttpGet("balance")]
    public async Task<IActionResult> ObtenerBalance([FromQuery] int usuarioId)
    {
        var resultado = await _obtenerBalanceHandler.Handle(new ObtenerBalanceQuery(usuarioId));

        if (resultado is null)
        {
            return NotFound();
        }

        return Ok(resultado);
    }
}