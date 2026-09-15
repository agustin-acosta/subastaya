namespace Application.Queries.ListarPujas;

public class ListarPujasQuery
{
    public int SubastaId { get; set; }

    public ListarPujasQuery(int subastaId)
    {
        SubastaId = subastaId;
    }
}