namespace Domain;

public class TransaccionLedger
{
    public int Id { get; set; }
    public int BilleteraId { get; set; }
    public TipoMovimiento Tipo { get; set; }
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }

    // opcional: no todo movimiento viene de una subasta (ej. un depósito manual), por eso es nullable.
    public int? SubastaId { get; set; }

    public Billetera Billetera { get; set; } = null!;
}