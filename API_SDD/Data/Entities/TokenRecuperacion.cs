using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSDD.Data.Entities;

/// <summary>
/// TB-AU-04: Tokens de recuperación de contraseña de un solo uso (30 min vigencia).
/// </summary>
[Table("tokens_recuperacion")]
public sealed class TokenRecuperacion
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [ForeignKey(nameof(Usuario))]
    public Guid UsuarioId { get; set; }

    public Usuario? Usuario { get; set; }

    [Required]
    [MaxLength(128)]
    public string TokenHash { get; set; } = string.Empty;

    public DateTime FechaExpiracion { get; set; }

    public bool Usado { get; set; } = false;

    public DateTime? FechaUso { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
