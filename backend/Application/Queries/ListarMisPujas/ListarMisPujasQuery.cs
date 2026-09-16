namespace Application.Queries.ListarMisPujas;

public class ListarMisPujasQuery
{
    public int UsuarioId { get; set; }

    public ListarMisPujasQuery(int usuarioId)
    {
        UsuarioId = usuarioId;
    }
}