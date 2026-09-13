namespace Application.Dtos;

public class PaginacionDto<T>
{
    public List<T> Items { get; set; } = new();
    public int PaginaActual { get; set; }
    public int TamanoPagina { get; set; }
    public int TotalItems { get; set; }
    public int TotalPaginas { get; set; }
}