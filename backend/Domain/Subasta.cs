namespace Domain;

public class Subasta
{
    public int Id { get; set; }
    public int VendedorId { get; set; }
    public int CategoriaId { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string UrlImagen { get; set; } = string.Empty;

    public decimal PrecioBase { get; set; }
    public decimal IncrementoMinimo { get; set; }

    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; } // se modifica por anti-sniping

    public EstadoSubasta Estado { get; set; } = EstadoSubasta.Programada;

    // redundante intencional: evita recalcular MAX(Puja.Monto) en cada listado del catálogo. solo se actualiza dentro de la misma transacción que crea una puja nueva
    public decimal? PujaActualMonto { get; set; }

    // optimistic locking: dos pujas simultáneas compiten por modificar esta misma fila (líder, monto, y posible extensión anti-sniping).
    public byte[] Version { get; set; } = Array.Empty<byte>();

    public Usuario Vendedor { get; set; } = null!;
    public Categoria Categoria { get; set; } = null!;
    public ICollection<Puja> Pujas { get; set; } = new List<Puja>();
}