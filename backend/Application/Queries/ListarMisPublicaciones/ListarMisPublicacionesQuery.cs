namespace Application.Queries.ListarMisPublicaciones;

public class ListarMisPublicacionesQuery
{
    public int VendedorId { get; set; }

    public ListarMisPublicacionesQuery(int vendedorId)
    {
        VendedorId = vendedorId;
    }
}