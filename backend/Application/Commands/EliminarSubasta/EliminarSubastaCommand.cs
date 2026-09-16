namespace Application.Commands.EliminarSubasta;

public class EliminarSubastaCommand
{
    public int SubastaId { get; set; }
    public int UsuarioSolicitanteId { get; set; }

    public EliminarSubastaCommand(int subastaId, int usuarioSolicitanteId)
    {
        SubastaId = subastaId;
        UsuarioSolicitanteId = usuarioSolicitanteId;
    }
}