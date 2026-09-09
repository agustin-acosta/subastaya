namespace Application.Commands.Ofertar;

public class OfertarCommand
{
    public int SubastaId { get; set; }
    public int CompradorId { get; set; }
    public decimal Monto { get; set; }

    public OfertarCommand(int subastaId, int compradorId, decimal monto)
    {
        SubastaId = subastaId;
        CompradorId = compradorId;
        Monto = monto;
    }
}