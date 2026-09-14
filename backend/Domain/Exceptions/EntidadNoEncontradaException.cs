namespace Domain.Exceptions;

public class EntidadNoEncontradaException : Exception
{
    public EntidadNoEncontradaException(string mensaje) : base(mensaje) { }
}