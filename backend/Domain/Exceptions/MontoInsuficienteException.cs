namespace Domain.Exceptions;

public class MontoInsuficienteException : Exception
{
    public MontoInsuficienteException(string mensaje) : base(mensaje) { }
}