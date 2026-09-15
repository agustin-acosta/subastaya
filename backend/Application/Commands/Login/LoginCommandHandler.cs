using Application.Dtos;
using Application.Interfaces;
using Domain.Exceptions;

namespace Application.Commands.Login;

public class LoginCommandHandler
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<TokenDto> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.ObtenerPorEmailAsync(command.Email, cancellationToken);

        // mismo mensaje de error tanto si el email no existe como si la contraseña está mal:
        // así nadie puede usar el error para adivinar qué emails están registrados.
        if (usuario is null || !_passwordHasher.Verificar(usuario.PasswordHash, command.Password))
        {
            throw new CredencialesInvalidasException("Email o contraseña incorrectos.");
        }

        var token = _jwtService.Generar(usuario);

        return new TokenDto
        {
            Token = token,
            UsuarioId = usuario.Id,
            Nombre = usuario.Nombre,
            Email = usuario.Email
        };
    }
}