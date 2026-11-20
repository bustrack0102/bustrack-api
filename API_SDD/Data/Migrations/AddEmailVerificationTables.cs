using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1861 // Prefer static readonly arrays (patrón de migración EF Core)

namespace ApiSDD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerificationTables : Migration
    {
        private static readonly string[] IndexColumns = ["UsuarioId", "FechaExpiracion", "Usado"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ArgumentNullException.ThrowIfNull(migrationBuilder);

            migrationBuilder.AddColumn<bool>(
                name: "CorreoVerificado",
                table: "usuarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaVerificacionCorreo",
                table: "usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tokens_verificacion_correo",
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
                    table.PrimaryKey("pk_tokens_verificacion_correo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tokens_verificacion_correo_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_tokens_verif_hash",
                table: "tokens_verificacion_correo",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_tokens_verif_usuario_vigentes",
                table: "tokens_verificacion_correo",
                columns: IndexColumns);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            ArgumentNullException.ThrowIfNull(migrationBuilder);

            migrationBuilder.DropTable(
                name: "tokens_verificacion_correo");

            migrationBuilder.DropColumn(
                name: "CorreoVerificado",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "FechaVerificacionCorreo",
                table: "usuarios");
        }
    }
}
