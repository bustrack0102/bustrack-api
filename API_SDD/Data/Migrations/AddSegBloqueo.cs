using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiSDD.Data.Migrations
{
    /// <summary>nv-05 T1: lockout table, no underscore per CA1707.</summary>
    public partial class AddSegBloqueo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ArgumentNullException.ThrowIfNull(migrationBuilder);
            migrationBuilder.CreateTable(
                name: "SDDSegBloqueo",
                columns: table => new
                {
                    strNombreUsuario = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    intIntentos = table.Column<int>(type: "int", nullable: false),
                    dteBloqueoHasta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SDDSegBloqueo", x => x.strNombreUsuario);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            ArgumentNullException.ThrowIfNull(migrationBuilder);
            migrationBuilder.DropTable(
                name: "SDDSegBloqueo");
        }
    }
}
