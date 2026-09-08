using Domain;

namespace Application.Interfaces;

public interface ISubastaRepository
{
    Task<List<Subasta>> ObtenerTodasAsync();
}