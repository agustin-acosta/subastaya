namespace Application.Dtos;

public class MiPujaDto
{
    public int SubastaId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string UrlImagen { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaFin { get; set; }
    public decimal PrecioActual { get; set; }
    public decimal MiMejorOferta { get; set; }
    public bool EstoyLiderando { get; set; }
}