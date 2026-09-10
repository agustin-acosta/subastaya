using Application.Commands.Depositar;
using Application.Dtos;
using Application.Queries.ObtenerBalance;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/wallet")]
public class WalletController : ControllerBase
{
    private readonly ObtenerBalanceQueryHandler _obtenerBalanceHandler;
    private readonly DepositarCommandHandler _depositarHandler;

    public WalletController(
        ObtenerBalanceQueryHandler obtenerBalanceHandler,
        DepositarCommandHandler depositarHandler)
    {
        _obtenerBalanceHandler = obtenerBalanceHandler;
        _depositarHandler = depositarHandler;
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

    [HttpPost("deposit")]
    public async Task<IActionResult> Depositar([FromBody] DepositarDto dto)
    {
        await _depositarHandler.Handle(new DepositarCommand(dto.UsuarioId, dto.Monto));
        return Ok();
    }
}