namespace Application.Commands.ActivarSubasta;

public class ActivarSubastaCommand
{
    public int SubastaId { get; }

    public ActivarSubastaCommand(int subastaId)
    {
        SubastaId = subastaId;
    }
}