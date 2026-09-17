namespace Application.Queries.ListarSubastas;

public class ListarSubastasQuery
{
    public string? Estado { get; set; }
    public int? CategoriaId { get; set; }
    public decimal? PrecioMin { get; set; }
    public decimal? PrecioMax { get; set; }
    public string? Busqueda { get; set; }
    public string? OrdenarPor { get; set; }
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }

    public ListarSubastasQuery(
        string? estado = null,
        int? categoriaId = null,
        decimal? precioMin = null,
        decimal? precioMax = null,
        string? busqueda = null,
        string? ordenarPor = null,
        int pagina = 1,
        int tamanoPagina = 10)
    {
        Estado = estado;
        CategoriaId = categoriaId;
        PrecioMin = precioMin;
        PrecioMax = precioMax;
        Busqueda = busqueda;
        OrdenarPor = ordenarPor;
        Pagina = pagina < 1 ? 1 : pagina;
        TamanoPagina = tamanoPagina is < 1 or > 50 ? 10 : tamanoPagina;
    }
}