using System.ComponentModel.DataAnnotations;

// SDD is the mandated entity/table prefix (constitution §5); Sonar S101 PascalCase cannot apply.
#pragma warning disable S101

namespace ApiSDD.Data.Entities;

/// <summary>
/// nv-04: persisted user. Wire names keep the legacy convention
/// (str/dte prefix); behavior stays in English.
/// </summary>
public sealed class SDDUsuario
{
    public int id { get; set; }

    public string strNombreUsuario { get; set; } = string.Empty;

    public string strPWD { get; set; } = string.Empty;

    public string strCorreoElectronico { get; set; } = string.Empty;

    public string strRol { get; set; } = "Usuario";

    public DateTime? dteFechaRegistro { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = [];
}
