using ApiTareas.Data;
using ApiTareas.Dtos;
using ApiTareas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiTareas.Controllers;

[ApiController]
[Route("api/tareas")]
[Produces("application/json")]
public class TareasController : ControllerBase
{
    private readonly TareasDbContext db;

    public TareasController(TareasDbContext db)
    {
        this.db = db;
    }

    /// <param name="estado">Filtra por estado: Pendiente, EnProgreso o Completada.</param>
    /// <param name="prioridad">Filtra por prioridad: Baja, Media o Alta.</param>
    /// <param name="categoriaId">Filtra por el Id de la categoría.</param>
    /// <response code="200">Lista de tareas (puede estar vacía).</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<TareaRespuesta>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] EstadoTarea? estado,
        [FromQuery] PrioridadTarea? prioridad,
        [FromQuery] int? categoriaId)
    {
        var consulta = db.Tareas.AsNoTracking().AsQueryable();

        if (estado is not null) consulta = consulta.Where(t => t.Estado == estado);
        if (prioridad is not null) consulta = consulta.Where(t => t.Prioridad == prioridad);
        if (categoriaId is not null) consulta = consulta.Where(t => t.CategoriaId == categoriaId);

        var tareas = await consulta
            .OrderBy(t => t.Id)
            .Select(TareaRespuesta.Proyeccion)
            .ToListAsync();

        return Ok(tareas);
    }

    /// <summary>Obtiene una tarea por su Id.</summary>
    /// <param name="id">Id de la tarea.</param>
    /// <response code="200">La tarea solicitada.</response>
    /// <response code="404">No existe una tarea con ese Id.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TareaRespuesta), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensajeRespuesta), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var tarea = await db.Tareas.AsNoTracking()
            .Where(t => t.Id == id)
            .Select(TareaRespuesta.Proyeccion)
            .FirstOrDefaultAsync();

        if (tarea is null)
        {
            return NotFound(new MensajeRespuesta("La tarea no existe."));
        }

        return Ok(tarea);
    }

    /// <summary>Crea una tarea.</summary>
    /// <param name="solicitud">Datos de la nueva tarea.</param>
    /// <response code="201">Tarea creada. El encabezado Location apunta a GET /api/tareas/{id}.</response>
    /// <response code="400">Datos inválidos, categoría inexistente o fecha límite anterior a hoy.</response>
    [HttpPost]
    [ProducesResponseType(typeof(TareaRespuesta), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] TareaSolicitud solicitud)
    {
        var categoria = await ValidarReglasAsync(solicitud);
        if (!ModelState.IsValid || categoria is null)
        {
            return ValidationProblem(ModelState);
        }

        var ahora = DateTime.UtcNow;
        var tarea = new Tarea
        {
            Titulo = solicitud.Titulo.Trim(),
            Descripcion = LimpiarTexto(solicitud.Descripcion),
            Prioridad = solicitud.Prioridad,
            Estado = solicitud.Estado,
            FechaLimite = solicitud.FechaLimite,
            FechaCreacion = ahora,
            FechaCompletada = solicitud.Estado == EstadoTarea.Completada ? ahora : null,
            CategoriaId = categoria.Id
        };

        db.Tareas.Add(tarea);
        await db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = tarea.Id },
            TareaRespuesta.Desde(tarea, categoria.Nombre));
    }

    /// <summary>Reemplaza los datos de una tarea existente.</summary>
    /// <param name="id">Id de la tarea a actualizar.</param>
    /// <param name="solicitud">Nuevos datos de la tarea.</param>
    /// <response code="200">Tarea actualizada.</response>
    /// <response code="400">Datos inválidos, categoría inexistente o fecha límite anterior a hoy.</response>
    /// <response code="404">No existe una tarea con ese Id.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TareaRespuesta), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MensajeRespuesta), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] TareaSolicitud solicitud)
    {
        var tarea = await db.Tareas.FindAsync(id);
        if (tarea is null)
        {
            return NotFound(new MensajeRespuesta("La tarea no existe."));
        }

        var categoria = await ValidarReglasAsync(solicitud);
        if (!ModelState.IsValid || categoria is null)
        {
            return ValidationProblem(ModelState);
        }

        tarea.Titulo = solicitud.Titulo.Trim();
        tarea.Descripcion = LimpiarTexto(solicitud.Descripcion);
        tarea.Prioridad = solicitud.Prioridad;
        tarea.Estado = solicitud.Estado;
        tarea.FechaLimite = solicitud.FechaLimite;
        tarea.CategoriaId = categoria.Id;
        // La fecha de cierre se conserva si ya estaba completada y se borra si se reabre.
        tarea.FechaCompletada = solicitud.Estado == EstadoTarea.Completada
            ? tarea.FechaCompletada ?? DateTime.UtcNow
            : null;

        await db.SaveChangesAsync();

        return Ok(TareaRespuesta.Desde(tarea, categoria.Nombre));
    }

    /// <summary>Elimina una tarea.</summary>
    /// <param name="id">Id de la tarea a eliminar.</param>
    /// <response code="204">Tarea eliminada (sin contenido).</response>
    /// <response code="404">No existe una tarea con ese Id.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(MensajeRespuesta), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id)
    {
        var tarea = await db.Tareas.FindAsync(id);
        if (tarea is null)
        {
            return NotFound(new MensajeRespuesta("La tarea no existe."));
        }

        db.Tareas.Remove(tarea);
        await db.SaveChangesAsync();

        return NoContent();
    }

    // Reglas que no se pueden expresar con atributos: la categoría debe existir y
    // una tarea pendiente o en progreso no puede tener una fecha límite que ya pasó.
    private async Task<Categoria?> ValidarReglasAsync(TareaSolicitud solicitud)
    {
        var categoria = await db.Categorias.FindAsync(solicitud.CategoriaId);
        if (categoria is null)
        {
            ModelState.AddModelError(nameof(TareaSolicitud.CategoriaId), "La categoría indicada no existe.");
        }

        if (solicitud.FechaLimite is { } fecha
            && solicitud.Estado != EstadoTarea.Completada
            && fecha < DateOnly.FromDateTime(DateTime.Today))
        {
            ModelState.AddModelError(nameof(TareaSolicitud.FechaLimite), "La fecha límite no puede ser anterior a hoy.");
        }

        return categoria;
    }

    private static string? LimpiarTexto(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}
