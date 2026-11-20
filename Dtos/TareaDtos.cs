using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using ApiTareas.Models;

namespace ApiTareas.Dtos;

/// <summary>Datos que envía el cliente para crear (POST) o reemplazar (PUT) una tarea.</summary>
public class TareaSolicitud
{
    /// <summary>Título de la tarea (3 a 120 caracteres).</summary>
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 120 caracteres.")]
    public string Titulo { get; init; } = string.Empty;

    /// <summary>Detalle opcional (hasta 500 caracteres).</summary>
    [StringLength(500, ErrorMessage = "La descripción no puede superar 500 caracteres.")]
    public string? Descripcion { get; init; }

    /// <summary>Baja, Media o Alta.</summary>
    [EnumDataType(typeof(PrioridadTarea), ErrorMessage = "La prioridad debe ser Baja, Media o Alta.")]
    public PrioridadTarea Prioridad { get; init; } = PrioridadTarea.Media;

    /// <summary>Pendiente, EnProgreso o Completada.</summary>
    [EnumDataType(typeof(EstadoTarea), ErrorMessage = "El estado debe ser Pendiente, EnProgreso o Completada.")]
    public EstadoTarea Estado { get; init; } = EstadoTarea.Pendiente;

    /// <summary>Fecha límite opcional (formato AAAA-MM-DD). No puede ser anterior a hoy, salvo en tareas completadas.</summary>
    public DateOnly? FechaLimite { get; init; }

    /// <summary>Id de la categoría a la que pertenece. Debe existir.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Indica el Id de una categoría válida.")]
    public int CategoriaId { get; init; }
}

/// <summary>Tarea tal como la devuelve la API.</summary>
public record TareaRespuesta(
    int Id,
    string Titulo,
    string? Descripcion,
    PrioridadTarea Prioridad,
    EstadoTarea Estado,
    DateOnly? FechaLimite,
    DateTime FechaCreacion,
    DateTime? FechaCompletada,
    int CategoriaId,
    string CategoriaNombre)
{
    /// <summary>Proyección reutilizable que EF Core traduce a SQL (solo lee las columnas necesarias).</summary>
    public static Expression<Func<Tarea, TareaRespuesta>> Proyeccion => t => new TareaRespuesta(
        t.Id, t.Titulo, t.Descripcion, t.Prioridad, t.Estado, t.FechaLimite,
        t.FechaCreacion, t.FechaCompletada, t.CategoriaId, t.Categoria.Nombre);

    /// <summary>Convierte una tarea ya cargada en memoria (usado después de crear o actualizar).</summary>
    public static TareaRespuesta Desde(Tarea t, string categoriaNombre) => new(
        t.Id, t.Titulo, t.Descripcion, t.Prioridad, t.Estado, t.FechaLimite,
        t.FechaCreacion, t.FechaCompletada, t.CategoriaId, categoriaNombre);
}
