using System.ComponentModel.DataAnnotations;

namespace ApiTareas.Dtos;

/// <summary>Datos que envía el cliente para crear una categoría.</summary>
public class CategoriaSolicitud
{
    /// <summary>Nombre único de la categoría (2 a 60 caracteres).</summary>
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 60 caracteres.")]
    public string Nombre { get; init; } = string.Empty;

    /// <summary>Descripción opcional (hasta 200 caracteres).</summary>
    [StringLength(200, ErrorMessage = "La descripción no puede superar 200 caracteres.")]
    public string? Descripcion { get; init; }
}

/// <summary>Categoría tal como la devuelve la API.</summary>
public record CategoriaRespuesta(int Id, string Nombre, string? Descripcion, int CantidadTareas);
