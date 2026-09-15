namespace Application.Dtos;

public class PujaListItemDto
{
    public decimal Monto { get; set; }
    public string CompradorSeudonimo { get; set; } = string.Empty;
    public DateTime FechaPuja { get; set; }
}