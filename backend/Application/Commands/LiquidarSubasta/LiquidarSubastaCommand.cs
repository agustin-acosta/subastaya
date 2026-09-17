namespace Application.Commands.LiquidarSubasta;

public class LiquidarSubastaCommand
{
    public int SubastaId { get; }

    public LiquidarSubastaCommand(int subastaId)
    {
        SubastaId = subastaId;
    }
}