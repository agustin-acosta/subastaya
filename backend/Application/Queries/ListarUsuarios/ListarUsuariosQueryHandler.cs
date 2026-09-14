using Application.Dtos;
using Application.Interfaces;

namespace Application.Queries.ListarUsuarios;

public class ListarUsuariosQueryHandler
{
    private readonly IUsuarioRepository _usuarioRepository;

    public ListarUsuariosQueryHandler(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<List<UsuarioListItemDto>> Handle(ListarUsuariosQuery query, CancellationToken cancellationToken)
    {
        var usuarios = await _usuarioRepository.ObtenerTodosAsync(cancellationToken);

        return usuarios.Select(u => new UsuarioListItemDto
        {
            Id = u.Id,
            Email = u.Email,
            Nombre = u.Nombre
        }).ToList();
    }
}