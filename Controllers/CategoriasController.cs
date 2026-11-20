using ApiTareas.Data;
using ApiTareas.Dtos;
using ApiTareas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ApiTareas.Controllers;

/// <summary>Recurso relacionado: categorías que agrupan las tareas.</summary>
[ApiController]
[Route("api/categorias")]
[Produces("application/json")]
public class CategoriasController : ControllerBase
{
    private readonly TareasDbContext db;

    public CategoriasController(TareasDbContext db)
    {
        this.db = db;
    }

    /// <summary>Lista las categorías con su cantidad de tareas.</summary>
    /// <response code="200">Lista de categorías.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<CategoriaRespuesta>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar()
    {
        var categorias = await db.Categorias.AsNoTracking()
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoriaRespuesta(c.Id, c.Nombre, c.Descripcion, c.Tareas.Count))
            .ToListAsync();

        return Ok(categorias);
    }

    /// <summary>Obtiene una categoría por su Id.</summary>
    /// <param name="id">Id de la categoría.</param>
    /// <response code="200">La categoría solicitada.</response>
    /// <response code="404">No existe una categoría con ese Id.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CategoriaRespuesta), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensajeRespuesta), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var categoria = await db.Categorias.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoriaRespuesta(c.Id, c.Nombre, c.Descripcion, c.Tareas.Count))
            .FirstOrDefaultAsync();

        if (categoria is null)
        {
            return NotFound(new MensajeRespuesta("La categoría no existe."));
        }

        return Ok(categoria);
    }

    /// <summary>Lista las tareas que pertenecen a una categoría.</summary>
    /// <param name="id">Id de la categoría.</param>
    /// <response code="200">Tareas de la categoría (puede estar vacía).</response>
    /// <response code="404">No existe una categoría con ese Id.</response>
    [HttpGet("{id:int}/tareas")]
    [ProducesResponseType(typeof(List<TareaRespuesta>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MensajeRespuesta), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarTareas(int id)
    {
        if (!await db.Categorias.AnyAsync(c => c.Id == id))
        {
            return NotFound(new MensajeRespuesta("La categoría no existe."));
        }

        var tareas = await db.Tareas.AsNoTracking()
            .Where(t => t.CategoriaId == id)
            .OrderBy(t => t.Id)
            .Select(TareaRespuesta.Proyeccion)
            .ToListAsync();

        return Ok(tareas);
    }

    /// <summary>Crea una categoría.</summary>
    /// <param name="solicitud">Datos de la nueva categoría.</param>
    /// <response code="201">Categoría creada. El encabezado Location apunta a GET /api/categorias/{id}.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="409">Ya existe una categoría con ese nombre.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CategoriaRespuesta), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MensajeRespuesta), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] CategoriaSolicitud solicitud)
    {
        var nombre = solicitud.Nombre.Trim();

        if (await db.Categorias.AnyAsync(c => c.Nombre == nombre))
        {
            return Conflict(new MensajeRespuesta("Ya existe una categoría con ese nombre."));
        }

        var categoria = new Categoria
        {
            Nombre = nombre,
            Descripcion = string.IsNullOrWhiteSpace(solicitud.Descripcion) ? null : solicitud.Descripcion.Trim()
        };

        db.Categorias.Add(categoria);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            // Otra solicitud creó el mismo nombre justo antes (índice único).
            return Conflict(new MensajeRespuesta("Ya existe una categoría con ese nombre."));
        }

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = categoria.Id },
            new CategoriaRespuesta(categoria.Id, categoria.Nombre, categoria.Descripcion, 0));
    }

    /// <summary>Elimina una categoría que no tenga tareas.</summary>
    /// <param name="id">Id de la categoría.</param>
    /// <response code="204">Categoría eliminada (sin contenido).</response>
    /// <response code="404">No existe una categoría con ese Id.</response>
    /// <response code="409">La categoría tiene tareas y no puede eliminarse.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(MensajeRespuesta), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MensajeRespuesta), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(int id)
    {
        var categoria = await db.Categorias.FindAsync(id);
        if (categoria is null)
        {
            return NotFound(new MensajeRespuesta("La categoría no existe."));
        }

        var tareas = await db.Tareas.CountAsync(t => t.CategoriaId == id);
        if (tareas > 0)
        {
            return Conflict(new MensajeRespuesta($"No se puede eliminar: la categoría tiene {tareas} tarea(s)."));
        }

        db.Categorias.Remove(categoria);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
