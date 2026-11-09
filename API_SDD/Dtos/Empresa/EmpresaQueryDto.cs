// ---------------------------------------------------------------------------
// DTO que representa los parámetros de búsqueda y paginación para el endpoint
// GET /api/v1/empresas.
// ---------------------------------------------------------------------------
namespace ApiSDD.Dtos.Empresa;

public sealed class EmpresaQueryDto
{
    // Texto de búsqueda libre (nombre, siglas, descripción).
    public string? Q { get; set; }

    // Id del medio de transporte (e.g. "bus", "combi").
    public string? MedioTransporteId { get; set; }

    // Nombre del municipio de cobertura.
    public string? Municipio { get; set; }

    // Calificación mínima (1..5).
    public double? CalificacionMin { get; set; }

    // Sólo activos.
    public bool? Activo { get; set; } = true;

    // Campo de ordenación: "calificacion", "nombre" o "rutas".
    public string? OrdenarPor { get; set; }

    // Paginación.
    public int Pagina { get; set; } = 1;
    public int TamanoPagina { get; set; } = 15;
}

