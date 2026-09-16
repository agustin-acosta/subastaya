namespace Application.Queries.ListarMovimientos;

public class ListarMovimientosQuery
{
    public int UsuarioId { get; set; }

    public ListarMovimientosQuery(int usuarioId)
    {
        UsuarioId = usuarioId;
    }
}