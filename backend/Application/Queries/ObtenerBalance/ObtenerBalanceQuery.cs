namespace Application.Queries.ObtenerBalance;

public class ObtenerBalanceQuery
{
    public int UsuarioId { get; set; }

    public ObtenerBalanceQuery(int usuarioId)
    {
        UsuarioId = usuarioId;
    }
}