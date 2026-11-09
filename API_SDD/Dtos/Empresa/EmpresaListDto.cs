// ---------------------------------------------------------------------------
// DTOs para los endpoints de Empresas.
// ---------------------------------------------------------------------------
namespace ApiSDD.Dtos.Empresa;

public sealed class EmpresaListDto
{
    public string Id { get; set; } = default!;
    public string Nombre { get; set; } = default!;
    public string? Siglas { get; set; }
    public string? Descripcion { get; set; }
    public string MunicipioBase { get; set; } = default!;
    public double CalificacionPromedio { get; set; }
    public int TotalEvaluaciones { get; set; }
    public IEnumerable<string> MediosTransporte { get; set; } = Array.Empty<string>();
    public int TotalRutasActivas { get; set; }
    public DateTime? FechaActualizacion { get; set; }
}

