using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSDD.Data.Entities;

/// <summary>
/// TB-EM-01: Catálogo de modalidades y medios de transporte público (Autobús, Combi, etc.).
/// </summary>
[Table("medios_transporte")]
public sealed class MedioTransporte
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string Clave { get; set; } = string.Empty;

    [Required]
    [MaxLength(60)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Descripcion { get; set; }

    [MaxLength(50)]
    public string? IconoClave { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime? FechaActualizacion { get; set; }

    public ICollection<Ruta> Rutas { get; set; } = new List<Ruta>();
}
