using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiSDD.Data.Migrations;

/// <summary>nv-04 T1: first entity table, no underscore per CA1707.</summary>
public partial class AddInitialSDDUsuario : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.CreateTable(
            name: "SDDUsuario",
            columns: table => new
            {
                id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                strNombreUsuario = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                strPWD = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                strCorreoElectronico = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                strRol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Usuario"),
                dteFechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SDDUsuario", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SDDUsuario_strCorreoElectronico",
            table: "SDDUsuario",
            column: "strCorreoElectronico",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_SDDUsuario_strNombreUsuario",
            table: "SDDUsuario",
            column: "strNombreUsuario",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.DropTable(name: "SDDUsuario");
    }
}
