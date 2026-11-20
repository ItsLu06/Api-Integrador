using System.Text.Json.Serialization;
using ApiTareas.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var cadena = builder.Configuration.GetConnectionString("TareasDb")
    ?? throw new InvalidOperationException("Falta la cadena de conexión TareasDb en appsettings.json.");

builder.Services.AddDbContext<TareasDbContext>(options =>
    options.UseSqlServer(cadena));

builder.Services.AddControllers(options =>
    {
        // Evita el mensaje extra "The solicitud field is required" cuando el JSON no se puede leer.
        // Los campos obligatorios se declaran de forma explícita con [Required] en los DTO.
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    // Los enums viajan como texto ("Alta", "Pendiente") y no como números.
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    // Respuesta 400 con los mensajes de validación en español.
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = contexto =>
        {
            var errores = contexto.ModelState
                .Where(e => e.Value is { Errors.Count: > 0 })
                .ToDictionary(
                    e => e.Key,
                    e => e.Value!.Errors.Select(x => TraducirError(x.ErrorMessage)).Distinct().ToArray());

            var problema = new ValidationProblemDetails(errores)
            {
                Status = StatusCodes.Status400BadRequest,
                Instance = contexto.HttpContext.Request.Path
            };

            return new BadRequestObjectResult(problema) { ContentTypes = { "application/problem+json" } };
        };
    });

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Tareas",
        Version = "v1",
        Description = "Gestión de tareas organizadas por categorías (ASP.NET Core, EF Core y SQL Server)."
    });

    // Muestra en Swagger UI los comentarios /// de controladores y DTO.
    var archivoXml = Path.Combine(AppContext.BaseDirectory, "ApiTareas.xml");
    if (File.Exists(archivoXml))
    {
        options.IncludeXmlComments(archivoXml);
    }
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Crea la base de datos y aplica la migración al iniciar (equivale a Update-Database).
    // Si la base ya existe y está al día, no hace nada.
    using (var scope = app.Services.CreateScope())
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        try
        {
            scope.ServiceProvider.GetRequiredService<TareasDbContext>().Database.Migrate();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "No se pudo aplicar la migración. Revisa la cadena de conexión TareasDb en appsettings.json y que SQL Server esté disponible.");
        }
    }

    app.UseSwagger();
    app.UseSwaggerUI(options => options.DocumentTitle = "API de Tareas");
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

// Traduce los mensajes técnicos del lector de JSON; los mensajes de los DTO ya están en español.
static string TraducirError(string mensaje)
{
    if (mensaje.Contains("could not be converted", StringComparison.OrdinalIgnoreCase))
    {
        return "El valor enviado no tiene el formato esperado (revisa los tipos, los valores permitidos y las fechas AAAA-MM-DD).";
    }

    if (mensaje.Contains("LineNumber", StringComparison.Ordinal))
    {
        return "El cuerpo de la solicitud no es un JSON válido.";
    }

    if (mensaje.Contains("request body is required", StringComparison.OrdinalIgnoreCase))
    {
        return "Se requiere un cuerpo JSON en la solicitud.";
    }

    return mensaje;
}
