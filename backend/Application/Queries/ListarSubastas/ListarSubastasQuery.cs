namespace Application.Queries.ListarSubastas;

public class ListarSubastasQuery
{
    public string? Estado { get; set; }
    public ListarSubastasQuery(string? estado = null)
    {
        Estado = estado;
    }
}