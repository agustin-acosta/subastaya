namespace Domain.Exceptions;

public class SubastaNoVigenteException : Exception
{
    public SubastaNoVigenteException(string mensaje) : base(mensaje) { }
}