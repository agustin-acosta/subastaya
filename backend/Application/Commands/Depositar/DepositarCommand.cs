namespace Application.Commands.Depositar;

public class DepositarCommand
{
    public int UsuarioId { get; set; }
    public decimal Monto { get; set; }

    public DepositarCommand(int usuarioId, decimal monto)
    {
        UsuarioId = usuarioId;
        Monto = monto;
    }
}