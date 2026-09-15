using Application.Commands.Login;
using Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly LoginCommandHandler _loginHandler;

    public AuthController(LoginCommandHandler loginHandler)
    {
        _loginHandler = loginHandler;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var resultado = await _loginHandler.Handle(new LoginCommand(dto.Email, dto.Password), cancellationToken);
        return Ok(resultado);
    }
}