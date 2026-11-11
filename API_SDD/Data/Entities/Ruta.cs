using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSDD.Data.Entities;

/// <summary>
/// Representa una ruta de transporte operada por una empresa con un medio de transporte.
/// </summary>
[Table("rutas")]
public sealed class Ruta
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [ForeignKey(nameof(Empresa))]
    public Guid EmpresaId { get; set; }

    public Empresa? Empresa { get; set; }

    [ForeignKey(nameof(MedioTransporte))]
    public int MedioTransporteId { get; set; }

    public MedioTransporte? MedioTransporte { get; set; }

    [Required]
    [MaxLength(30)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ParadaOrigen { get; set; }

    [MaxLength(100)]
    public string? ParadaDestino { get; set; }

    [Column(TypeName = "decimal(8,2)")]
    public decimal TarifaBaseMxn { get; set; } = 0.00m;

    [MaxLength(10)]
    public string? PrimerHorario { get; set; }

    [MaxLength(10)]
    public string? UltimoHorario { get; set; }

    public int FrecuenciaMinutos { get; set; } = 15;

    [MaxLength(50)]
    public string DiasOperacion { get; set; } = "L-D";

    public bool Activo { get; set; } = true;

    public DateTime? FechaActualizacion { get; set; }
}
