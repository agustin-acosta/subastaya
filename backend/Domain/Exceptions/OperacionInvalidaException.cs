namespace Domain.Exceptions;

public class OperacionInvalidaException : Exception
{
    public OperacionInvalidaException(string mensaje) : base(mensaje) { }
}