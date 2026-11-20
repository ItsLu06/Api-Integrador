namespace ApiTareas.Models;

public class Tarea
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public PrioridadTarea Prioridad { get; set; }
    public EstadoTarea Estado { get; set; }
    public DateOnly? FechaLimite { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaCompletada { get; set; }

    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;
}
