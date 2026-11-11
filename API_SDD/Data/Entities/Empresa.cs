using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiSDD.Data.Entities;

/// <summary>
/// TB-EM-02: Empresas, líneas o asociaciones de transporte público.
/// </summary>
[Table("empresas")]
public sealed class Empresa
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(40)]
    public string? Siglas { get; set; }

    public string? Descripcion { get; set; }

    [MaxLength(80)]
    public string? MunicipioBase { get; set; }

    [MaxLength(30)]
    public string? TelefonoContacto { get; set; }

    [MaxLength(200)]
    public string? SitioWeb { get; set; }

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    [MaxLength(100)]
    public string? HorarioAtencion { get; set; }

    [Column(TypeName = "decimal(3,2)")]
    public decimal CalificacionPromedio { get; set; } = 0.00m;

    public int TotalCalificaciones { get; set; } = 0;

    public Guid? FuenteDatoId { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime? FechaActualizacion { get; set; }

    public ICollection<Ruta> Rutas { get; set; } = new List<Ruta>();
}
