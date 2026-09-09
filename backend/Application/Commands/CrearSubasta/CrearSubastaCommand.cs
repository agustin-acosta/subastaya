namespace Application.Commands.CrearSubasta;

public class CrearSubastaCommand
{
    public int VendedorId { get; set; }
    public int CategoriaId { get; set; }
    public string Titulo { get; set; }
    public string Descripcion { get; set; }
    public string UrlImagen { get; set; }
    public decimal PrecioBase { get; set; }
    public decimal IncrementoMinimo { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }

    public CrearSubastaCommand(
        int vendedorId, int categoriaId, string titulo, string descripcion,
        string urlImagen, decimal precioBase, decimal incrementoMinimo,
        DateTime fechaInicio, DateTime fechaFin)
    {
        VendedorId = vendedorId;
        CategoriaId = categoriaId;
        Titulo = titulo;
        Descripcion = descripcion;
        UrlImagen = urlImagen;
        PrecioBase = precioBase;
        IncrementoMinimo = incrementoMinimo;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
    }
}