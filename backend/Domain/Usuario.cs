using System;
namespace Domain;

public class Usuario
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }

    // relacion de navegacion 1:1 con billetera.
    public Billetera? Billetera { get; set; }
}