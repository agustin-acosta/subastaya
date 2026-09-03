namespace Domain;

public class Billetera
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public decimal SaldoTotal { get; set; }
    public decimal SaldoRetenido { get; set; }

    // calculado, no guardado en la base de datos
    public decimal SaldoDisponible => SaldoTotal - SaldoRetenido;

    // necesario para optimistic locking (se mapea como rowversion en sql server cuando configuremos ef core en infrastructure).
    public byte[] Version { get; set; } = Array.Empty<byte>();

    public Usuario Usuario { get; set; } = null!;
}