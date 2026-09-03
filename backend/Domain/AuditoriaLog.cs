namespace Domain;

public class AuditoriaLog
{
    public int Id { get; set; }

    // generico a proposito: permite auditar cualquier tipo de entidad presente o futura sin fks fisicas por cada una
    public string Entidad { get; set; } = string.Empty;   // ej: "Subasta"
    public int EntidadId { get; set; }

    public string Accion { get; set; } = string.Empty;    // ej: "EXTENSION_TIEMPO"
    public int? UsuarioId { get; set; }                    // null si lo generó el worker
    public string DetalleJson { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }

    // sin navegacion a Usuario por fk fisica, porque UsuarioId puede referenciar distintos orígenes (o ninguno). si mas adelante
    // necesitamos navegar desde AuditoriaLog a Usuario cuando exista, lo resolvemos en la capa de Application, no aca.
}