namespace Application.Commands.ModificarSubasta;

public class ModificarSubastaCommand
{
    public int SubastaId { get; set; }
    public string Titulo { get; set; }
    public string Descripcion { get; set; }
    public string UrlImagen { get; set; }
    public decimal PrecioBase { get; set; }
    public decimal IncrementoMinimo { get; set; }
    public DateTime FechaFin { get; set; }

    public ModificarSubastaCommand(
        int subastaId, string titulo, string descripcion, string urlImagen,
        decimal precioBase, decimal incrementoMinimo, DateTime fechaFin)
    {
        SubastaId = subastaId;
        Titulo = titulo;
        Descripcion = descripcion;
        UrlImagen = urlImagen;
        PrecioBase = precioBase;
        IncrementoMinimo = incrementoMinimo;
        FechaFin = fechaFin;
    }
}