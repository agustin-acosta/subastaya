namespace Application.Dtos;

public class SubastaListItemDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string UrlImagen { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public decimal? PujaActualMonto { get; set; }
    public decimal PrecioBase { get; set; }
    public DateTime FechaFin { get; set; }
    public string Estado { get; set; } = string.Empty;
}