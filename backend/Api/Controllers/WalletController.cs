using System.Security.Claims;
using Application.Commands.Depositar;
using Application.Dtos;
using Application.Queries.ListarMovimientos;
using Application.Queries.ObtenerBalance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/wallet")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly ObtenerBalanceQueryHandler _obtenerBalanceHandler;
    private readonly DepositarCommandHandler _depositarHandler;
    private readonly ListarMovimientosQueryHandler _listarMovimientosHandler;

    public WalletController(
        ObtenerBalanceQueryHandler obtenerBalanceHandler,
        DepositarCommandHandler depositarHandler,
        ListarMovimientosQueryHandler listarMovimientosHandler)
    {
        _obtenerBalanceHandler = obtenerBalanceHandler;
        _depositarHandler = depositarHandler;
        _listarMovimientosHandler = listarMovimientosHandler;
    }

    private int UsuarioActualId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("balance")]
    public async Task<IActionResult> ObtenerBalance(CancellationToken cancellationToken)
    {
        var resultado = await _obtenerBalanceHandler.Handle(new ObtenerBalanceQuery(UsuarioActualId), cancellationToken);
        if (resultado is null)
        {
            return NotFound();
        }
        return Ok(resultado);
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Depositar([FromBody] DepositarDto dto, CancellationToken cancellationToken)
    {
        await _depositarHandler.Handle(new DepositarCommand(UsuarioActualId, dto.Monto), cancellationToken);
        return Ok();
    }

    [HttpGet("movimientos")]
    public async Task<IActionResult> ListarMovimientos(CancellationToken cancellationToken)
    {
        var resultado = await _listarMovimientosHandler.Handle(new ListarMovimientosQuery(UsuarioActualId), cancellationToken);
        return Ok(resultado);
    }
}