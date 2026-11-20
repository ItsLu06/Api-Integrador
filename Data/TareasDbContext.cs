using ApiTareas.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiTareas.Data;

public class TareasDbContext : DbContext
{
    public TareasDbContext(DbContextOptions<TareasDbContext> options) : base(options) { }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Tarea> Tareas => Set<Tarea>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(categoria =>
        {
            categoria.Property(c => c.Nombre).HasMaxLength(60);
            categoria.Property(c => c.Descripcion).HasMaxLength(200);
            categoria.HasIndex(c => c.Nombre).IsUnique();

            // Datos de prueba (se insertan con la migración)
            categoria.HasData(
                new Categoria { Id = 1, Nombre = "Estudio", Descripcion = "Trabajos, lecturas y exámenes" },
                new Categoria { Id = 2, Nombre = "Trabajo", Descripcion = "Pendientes laborales" },
                new Categoria { Id = 3, Nombre = "Personal", Descripcion = "Hogar, salud y trámites" }
            );
        });

        modelBuilder.Entity<Tarea>(tarea =>
        {
            tarea.Property(t => t.Titulo).HasMaxLength(120);
            tarea.Property(t => t.Descripcion).HasMaxLength(500);

            // Los enums se guardan como texto legible ("Pendiente", "Alta"...) en lugar de números.
            tarea.Property(t => t.Prioridad).HasConversion<string>().HasMaxLength(10);
            tarea.Property(t => t.Estado).HasConversion<string>().HasMaxLength(15);

            tarea.HasOne(t => t.Categoria)
                .WithMany(c => c.Tareas)
                .HasForeignKey(t => t.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            tarea.HasData(
                new Tarea
                {
                    Id = 1, Titulo = "Entregar la API Integrador", Descripcion = "Subir el ZIP, el PDF de evidencias y el README a Teams",
                    Prioridad = PrioridadTarea.Alta, Estado = EstadoTarea.EnProgreso, FechaLimite = new DateOnly(2026, 11, 20),
                    FechaCreacion = new DateTime(2026, 11, 15, 12, 0, 0), CategoriaId = 1
                },
                new Tarea
                {
                    Id = 2, Titulo = "Estudiar para el examen de Base de Datos", Descripcion = "Repasar normalización y consultas",
                    Prioridad = PrioridadTarea.Media, Estado = EstadoTarea.Pendiente, FechaLimite = new DateOnly(2026, 12, 5),
                    FechaCreacion = new DateTime(2026, 11, 15, 12, 0, 0), CategoriaId = 1
                },
                new Tarea
                {
                    Id = 3, Titulo = "Preparar la presentación del proyecto", Descripcion = null,
                    Prioridad = PrioridadTarea.Media, Estado = EstadoTarea.Pendiente, FechaLimite = new DateOnly(2026, 12, 10),
                    FechaCreacion = new DateTime(2026, 11, 16, 9, 30, 0), CategoriaId = 2
                },
                new Tarea
                {
                    Id = 4, Titulo = "Pagar el servicio de internet", Descripcion = "Vence a fin de mes",
                    Prioridad = PrioridadTarea.Baja, Estado = EstadoTarea.Pendiente, FechaLimite = new DateOnly(2026, 11, 30),
                    FechaCreacion = new DateTime(2026, 11, 16, 9, 30, 0), CategoriaId = 3
                },
                new Tarea
                {
                    Id = 5, Titulo = "Instalar SQL Server y Visual Studio", Descripcion = "Ya quedó listo el entorno",
                    Prioridad = PrioridadTarea.Baja, Estado = EstadoTarea.Completada, FechaLimite = null,
                    FechaCreacion = new DateTime(2026, 11, 10, 8, 0, 0), FechaCompletada = new DateTime(2026, 11, 11, 18, 0, 0), CategoriaId = 1
                }
            );
        });
    }
}
