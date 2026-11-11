using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSDD.Data.Entities;

/// <summary>
/// TB-AU-02: Tabla maestra de cuentas de usuarios y administradores.
/// </summary>
[Table("usuarios")]
public sealed class Usuario
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [ForeignKey(nameof(Rol))]
    public int RolId { get; set; }

    public Rol? Rol { get; set; }

    [Required]
    [MaxLength(120)]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Correo { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public bool ConsentimientoSensores { get; set; } = false;

    public DateTime? FechaAvisoPrivacidad { get; set; }

    public DateTime? UltimoAcceso { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public ICollection<TokenRecuperacion> TokensRecuperacion { get; set; } = new List<TokenRecuperacion>();
}
