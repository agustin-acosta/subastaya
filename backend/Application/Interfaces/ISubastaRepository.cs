using Domain;

namespace Application.Interfaces;

public interface ISubastaRepository
{
    Task<List<Subasta>> ObtenerTodasAsync(
        string? estado, int? categoriaId, decimal? precioMin, decimal? precioMax, string? busqueda, string? ordenarPor,
        int pagina, int tamanoPagina, CancellationToken cancellationToken);
    Task<int> ContarAsync(string? estado, int? categoriaId, decimal? precioMin, decimal? precioMax, string? busqueda, CancellationToken cancellationToken);
    Task<Subasta?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken);
    Task<Puja?> ObtenerPujaConMayorMontoAsync(int subastaId, CancellationToken cancellationToken);
    Task<List<Puja>> ObtenerPujasPorSubastaAsync(int subastaId, CancellationToken cancellationToken);
    Task<List<Subasta>> ObtenerVencidasSinLiquidarAsync(DateTime ahora, CancellationToken cancellationToken);
    Task<List<Subasta>> ObtenerProgramadasParaActivarAsync(DateTime ahora, CancellationToken cancellationToken);
    Task<List<Subasta>> ObtenerPorVendedorAsync(int vendedorId, CancellationToken cancellationToken);
    Task<List<Subasta>> ObtenerConPujaDeUsuarioAsync(int usuarioId, CancellationToken cancellationToken);
    void Agregar(Subasta subasta);
    void ActualizarSubasta(Subasta subasta);
    void AgregarPuja(Puja puja);
    void AgregarAuditoria(AuditoriaLog log);
    void Eliminar(Subasta subasta);
}