// ---------------------------------------------------------------------------
// DTOs para los endpoints de Empresas (detalle y query).
// ---------------------------------------------------------------------------
namespace ApiSDD.Dtos.Empresa;

public sealed class EmpresaDetailDto
{
    public string Id { get; set; } = default!;
    public string Nombre { get; set; } = default!;
    public string? Siglas { get; set; }
    public string? Descripcion { get; set; }
    public string TelefonoContacto { get; set; } = default!;
    public string? SitioWeb { get; set; }
    public string? HorarioAtencion { get; set; }
    public double CalificacionPromedio { get; set; }
    public int TotalEvaluaciones { get; set; }
    public IEnumerable<string> MediosTransporte { get; set; } = Array.Empty<string>();
    public IEnumerable<RutaDto> Rutas { get; set; } = Array.Empty<RutaDto>();
    public IEnumerable<DestinoDto> DestinosAsociados { get; set; } = Array.Empty<DestinoDto>();
    public DateTime? FechaUltimaActualizacion { get; set; }
}

public sealed class RutaDto
{
    public string Id { get; set; } = default!;
    public string Nombre { get; set; } = default!;
    public string MedioTransporteId { get; set; } = default!;
    public string Sentido { get; set; } = default!;
    public decimal PrecioBase { get; set; }
    // Otros campos pueden añadirse según necesidad.
}

public sealed class DestinoDto
{
    public string Id { get; set; } = default!;
    public string Nombre { get; set; } = default!;
    // Información adicional opcional.
}

