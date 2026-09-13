namespace Application.Queries.ListarSubastas;

public class ListarSubastasQuery
{
    public string? Estado { get; set; }
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }

    public ListarSubastasQuery(string? estado = null, int pagina = 1, int tamanoPagina = 10)
    {
        Estado = estado;
        Pagina = pagina < 1 ? 1 : pagina;
        TamanoPagina = tamanoPagina is < 1 or > 50 ? 10 : tamanoPagina;
    }
}