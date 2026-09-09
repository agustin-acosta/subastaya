namespace Domain.Exceptions;

public class SaldoInsuficienteException : Exception
{
    public SaldoInsuficienteException(string mensaje) : base(mensaje) { }
}