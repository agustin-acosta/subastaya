namespace Application.Commands.EliminarSubasta;

public class EliminarSubastaCommand
{
    public int SubastaId { get; set; }

    public EliminarSubastaCommand(int subastaId)
    {
        SubastaId = subastaId;
    }
}