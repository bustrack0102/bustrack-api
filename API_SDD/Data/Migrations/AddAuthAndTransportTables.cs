using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814, CA1861 // Prefer jagged arrays over multidimensional / static readonly arrays (patrón autogenerado de migraciones EF)

namespace ApiSDD.Data.Migrations
{
    /// <summary>
    /// Crea las tablas de autenticación (TB-AU-01 a TB-AU-04) y transporte
    /// (TB-EM-01, TB-EM-02 y rutas) con sus índices y el seed de roles.
    /// Las tablas legacy SDDUsuario/SDDSegBloqueo pertenecen a migraciones previas.
    /// </summary>
    public partial class AddAuthAndTransportTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ArgumentNullException.ThrowIfNull(migrationBuilder);
            migrationBuilder.CreateTable(
                name: "empresas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Siglas = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MunicipioBase = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    TelefonoContacto = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    SitioWeb = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HorarioAtencion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CalificacionPromedio = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    TotalCalificaciones = table.Column<int>(type: "int", nullable: false),
                    FuenteDatoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_empresas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "medios_transporte",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Clave = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IconoClave = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medios_transporte", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rutas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedioTransporteId = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ParadaOrigen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ParadaDestino = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TarifaBaseMxn = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    PrimerHorario = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    UltimoHorario = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    FrecuenciaMinutos = table.Column<int>(type: "int", nullable: false),
                    DiasOperacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rutas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rutas_empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rutas_medios_transporte_MedioTransporteId",
                        column: x => x.MedioTransporteId,
                        principalTable: "medios_transporte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RolId = table.Column<int>(type: "int", nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    ConsentimientoSensores = table.Column<bool>(type: "bit", nullable: false),
                    FechaAvisoPrivacidad = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UltimoAcceso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_usuarios_roles_RolId",
                        column: x => x.RolId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DispositivoInfo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FechaExpiracion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Revocado = table.Column<bool>(type: "bit", nullable: false),
                    FechaRevocacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReemplazadoPorTokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tokens_recuperacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Usado = table.Column<bool>(type: "bit", nullable: false),
                    FechaUso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tokens_recuperacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tokens_recuperacion_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "Id", "Descripcion", "FechaCreacion", "Nombre" },
                values: new object[,]
                {
                    { 1, "Usuario final del sistema", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Usuario" },
                    { 2, "Administrador del sistema", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Administrador" }
                });

            migrationBuilder.CreateIndex(
                name: "idx_empresas_activa_calificacion",
                table: "empresas",
                columns: new[] { "Activo", "CalificacionPromedio", "TotalCalificaciones" });

            migrationBuilder.CreateIndex(
                name: "idx_empresas_municipio",
                table: "empresas",
                column: "MunicipioBase");

            migrationBuilder.CreateIndex(
                name: "uk_empresas_nombre",
                table: "empresas",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_medios_transporte_activo",
                table: "medios_transporte",
                columns: new[] { "Activo", "Nombre" });

            migrationBuilder.CreateIndex(
                name: "idx_medios_transporte_clave",
                table: "medios_transporte",
                column: "Clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uk_medios_transporte_nombre",
                table: "medios_transporte",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_refresh_tokens_expiracion",
                table: "refresh_tokens",
                column: "FechaExpiracion");

            migrationBuilder.CreateIndex(
                name: "idx_refresh_tokens_token_hash",
                table: "refresh_tokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_refresh_tokens_usuario_activos",
                table: "refresh_tokens",
                columns: new[] { "UsuarioId", "FechaExpiracion", "Revocado" });

            migrationBuilder.CreateIndex(
                name: "uk_roles_nombre",
                table: "roles",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_rutas_empresa_medio",
                table: "rutas",
                columns: new[] { "EmpresaId", "MedioTransporteId" });

            migrationBuilder.CreateIndex(
                name: "IX_rutas_MedioTransporteId",
                table: "rutas",
                column: "MedioTransporteId");

            migrationBuilder.CreateIndex(
                name: "idx_tokens_recup_hash",
                table: "tokens_recuperacion",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_tokens_recup_usuario_vigentes",
                table: "tokens_recuperacion",
                columns: new[] { "UsuarioId", "FechaExpiracion", "Usado" });

            migrationBuilder.CreateIndex(
                name: "idx_usuarios_rol_activo",
                table: "usuarios",
                columns: new[] { "RolId", "Activo", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "uk_usuarios_correo",
                table: "usuarios",
                column: "Correo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            ArgumentNullException.ThrowIfNull(migrationBuilder);
            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "rutas");

            migrationBuilder.DropTable(
                name: "tokens_recuperacion");

            migrationBuilder.DropTable(
                name: "empresas");

            migrationBuilder.DropTable(
                name: "medios_transporte");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
