using System.ComponentModel.DataAnnotations;

// SDD is the mandated entity/table prefix (constitution §5); Sonar S101 PascalCase cannot apply.
#pragma warning disable S101

namespace ApiSDD.Data.Entities;

/// <summary>
/// nv-05: login lockout state per user name. Persisted in SQL, fail-closed on concurrency.
/// </summary>
public sealed class SDDSegBloqueo
{
    public string strNombreUsuario { get; set; } = string.Empty;

    public int intIntentos { get; set; }

    public DateTime? dteBloqueoHasta { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = [];
}
