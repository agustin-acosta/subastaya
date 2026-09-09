using Domain;

namespace Application.Interfaces;

public interface ISubastaRepository
{
    Task<List<Subasta>> ObtenerTodasAsync(string? estado);
    Task<Subasta?> ObtenerPorIdAsync(int id);
    Task<Puja?> ObtenerUltimaPujaAsync(int subastaId);
    void ActualizarSubasta(Subasta subasta);
    void AgregarPuja(Puja puja);
    void AgregarAuditoria(AuditoriaLog log);
    void Agregar(Subasta subasta);
}