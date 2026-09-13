using Domain;

namespace Application.Interfaces;

public interface ISubastaRepository
{
    Task<List<Subasta>> ObtenerTodasAsync(string? estado, int pagina, int tamanoPagina);
    Task<int> ContarAsync(string? estado);
    Task<Subasta?> ObtenerPorIdAsync(int id);
    Task<Puja?> ObtenerUltimaPujaAsync(int subastaId);
    Task<List<Subasta>> ObtenerVencidasSinLiquidarAsync(DateTime ahora);
    Task<Puja?> ObtenerPujaGanadoraAsync(int subastaId);
<<<<<<< HEAD
    void Agregar(Subasta subasta);
    void ActualizarSubasta(Subasta subasta);
    void AgregarPuja(Puja puja);
    void AgregarAuditoria(AuditoriaLog log);
=======
    void ActualizarSubasta(Subasta subasta);
    void AgregarPuja(Puja puja);
    void AgregarAuditoria(AuditoriaLog log);
    void Agregar(Subasta subasta);
>>>>>>> d7b8f7fad8cac88cfd89b395df862ee7a219c6a7
    void Eliminar(Subasta subasta);
}