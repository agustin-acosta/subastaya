namespace Domain;

public class Billetera
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public decimal SaldoTotal { get; set; }
    public decimal SaldoRetenido { get; set; }

    public decimal SaldoDisponible => SaldoTotal - SaldoRetenido;

    public byte[] Version { get; set; } = Array.Empty<byte>();

    public Usuario Usuario { get; set; } = null!;
}