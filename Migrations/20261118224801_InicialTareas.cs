using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ApiTareas.Migrations
{
    /// <inheritdoc />
    public partial class InicialTareas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tareas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Prioridad = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    FechaLimite = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCompletada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CategoriaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tareas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tareas_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, "Trabajos, lecturas y exámenes", "Estudio" },
                    { 2, "Pendientes laborales", "Trabajo" },
                    { 3, "Hogar, salud y trámites", "Personal" }
                });

            migrationBuilder.InsertData(
                table: "Tareas",
                columns: new[] { "Id", "CategoriaId", "Descripcion", "Estado", "FechaCompletada", "FechaCreacion", "FechaLimite", "Prioridad", "Titulo" },
                values: new object[,]
                {
                    { 1, 1, "Subir el ZIP, el PDF de evidencias y el README a Teams", "EnProgreso", null, new DateTime(2026, 11, 15, 12, 0, 0, 0, DateTimeKind.Unspecified), new DateOnly(2026, 11, 20), "Alta", "Entregar la API Integrador" },
                    { 2, 1, "Repasar normalización y consultas", "Pendiente", null, new DateTime(2026, 11, 15, 12, 0, 0, 0, DateTimeKind.Unspecified), new DateOnly(2026, 12, 5), "Media", "Estudiar para el examen de Base de Datos" },
                    { 3, 2, null, "Pendiente", null, new DateTime(2026, 11, 16, 9, 30, 0, 0, DateTimeKind.Unspecified), new DateOnly(2026, 12, 10), "Media", "Preparar la presentación del proyecto" },
                    { 4, 3, "Vence a fin de mes", "Pendiente", null, new DateTime(2026, 11, 16, 9, 30, 0, 0, DateTimeKind.Unspecified), new DateOnly(2026, 11, 30), "Baja", "Pagar el servicio de internet" },
                    { 5, 1, "Ya quedó listo el entorno", "Completada", new DateTime(2026, 11, 11, 18, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 11, 10, 8, 0, 0, 0, DateTimeKind.Unspecified), null, "Baja", "Instalar SQL Server y Visual Studio" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_Nombre",
                table: "Categorias",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_CategoriaId",
                table: "Tareas",
                column: "CategoriaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tareas");

            migrationBuilder.DropTable(
                name: "Categorias");
        }
    }
}
