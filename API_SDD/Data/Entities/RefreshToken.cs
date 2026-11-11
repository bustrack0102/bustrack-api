using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSDD.Data.Entities;

/// <summary>
/// TB-AU-03: Control de sesiones activas y renovación de tokens JWT con rotación segura.
/// </summary>
[Table("refresh_tokens")]
public sealed class RefreshToken
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [ForeignKey(nameof(Usuario))]
    public Guid UsuarioId { get; set; }

    public Usuario? Usuario { get; set; }

    [Required]
    [MaxLength(128)]
    public string TokenHash { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? DispositivoInfo { get; set; }

    public DateTime FechaExpiracion { get; set; }

    public bool Revocado { get; set; } = false;

    public DateTime? FechaRevocacion { get; set; }

    public Guid? ReemplazadoPorTokenId { get; set; }
}
