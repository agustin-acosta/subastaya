namespace Application.Queries.ListarPujas;

public class ListarPujasQuery
{
    public int SubastaId { get; set; }
    public int? UsuarioActualId { get; set; }

    public ListarPujasQuery(int subastaId, int? usuarioActualId)
    {
        SubastaId = subastaId;
        UsuarioActualId = usuarioActualId;
    }
}