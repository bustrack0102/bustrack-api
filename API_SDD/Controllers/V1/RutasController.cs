using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiSDD.Controllers.V1;

/// <summary>
/// Controlador de rutas de transporte público (Specs 19 y 20).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/rutas")]
public sealed class RutasController : ControllerBase
{
    private static readonly IReadOnlyList<RutaItemResponse> SeedRutas =
    [
        new(
            Guid.Parse("10000000-0000-0000-0000-000000000001"),
            "R-01",
            "Pachuca – Tula Directo Express",
            "Autotransportes Valle del Mezquital",
            "Autobús Directo",
            "Central de Autobuses de Pachuca",
            "Central de Autobuses de Tula",
            65.00m,
            "05:00",
            "22:30",
            15,
            "Lunes a Domingo",
            true
        ),
        new(
            Guid.Parse("10000000-0000-0000-0000-000000000002"),
            "R-02",
            "Tula – Tepeji Centro Metropolitano",
            "Autotransportes Tepeji del Río",
            "Microbús Urbano",
            "Tula Centro (Plaza Juárez)",
            "Tepeji Centro (Jardín Municipal)",
            22.00m,
            "05:30",
            "22:00",
            10,
            "Lunes a Domingo",
            true
        ),
        new(
            Guid.Parse("10000000-0000-0000-0000-000000000003"),
            "R-03",
            "Tula – Refinería Miguel Hidalgo",
            "Autotransportes Valle del Mezquital",
            "Microbús Urbano",
            "Central de Autobuses de Tula",
            "Puerta 1 Refinería",
            15.00m,
            "06:00",
            "21:00",
            12,
            "Lunes a Sábado",
            true
        ),
        new(
            Guid.Parse("10000000-0000-0000-0000-000000000004"),
            "R-04",
            "Pachuca – Mineral del Monte Turístico",
            "Flecha Roja Hidalgo",
            "Autobús Directo",
            "Plaza Juárez Pachuca",
            "Monumento al Minero Real del Monte",
            30.00m,
            "06:30",
            "20:00",
            20,
            "Lunes a Domingo",
            true
        )
    ];

    private static readonly IReadOnlyList<ParadaItemResponse> SeedParadas =
    [
        new("P-01", "Central de Autobuses de Pachuca", 20.1012, -98.7523, 1),
        new("P-02", "Plaza Juárez Pachuca", 20.1245, -98.7321, 2),
        new("P-03", "Puente Actopan Troncal", 20.2687, -98.9431, 3),
        new("P-04", "Crucero Cruz Azul", 20.0345, -99.2891, 4),
        new("P-05", "Central de Autobuses de Tula", 20.0512, -99.3421, 5)
    ];

    // GET /api/v1/rutas
    [HttpGet]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public IActionResult GetRutas(
        [FromQuery] string? empresa = null,
        [FromQuery] string? destino = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20)
    {
        var query = SeedRutas.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(empresa))
        {
            query = query.Where(r => r.Empresa.Contains(empresa.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(destino))
        {
            var dest = destino.Trim();
            query = query.Where(r => r.ParadaDestino.Contains(dest, StringComparison.OrdinalIgnoreCase));
        }

        var filteredList = query.ToList();
        var total = filteredList.Count;
        var p = Math.Max(1, pagina);
        var s = Math.Clamp(tamanoPagina, 1, 100);
        var items = filteredList.Skip((p - 1) * s).Take(s).ToList();

        Response.Headers.Append("Cache-Control", "public, max-age=60");
        return Ok(new
        {
            totalRegistros = total,
            paginaActual = p,
            tamanoPagina = s,
            items
        });
    }

    // GET /api/v1/rutas/{id}
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public IActionResult GetRutaPorId([FromRoute] Guid id)
    {
        var ruta = SeedRutas.FirstOrDefault(r => r.Id == id);
        if (ruta is null)
        {
            return NotFound(new { error = "Ruta no encontrada.", status = 404 });
        }

        return Ok(ruta);
    }

    // GET /api/v1/rutas/{id}/paradas (Spec 20)
    [HttpGet("{id:guid}/paradas")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public IActionResult GetParadasPorRuta([FromRoute] Guid id)
    {
        var ruta = SeedRutas.FirstOrDefault(r => r.Id == id);
        if (ruta is null)
        {
            return NotFound(new { error = "Ruta no encontrada.", status = 404 });
        }

        return Ok(new
        {
            rutaId = id,
            rutaNombre = ruta.Nombre,
            totalParadas = SeedParadas.Count,
            paradas = SeedParadas
        });
    }
}

public sealed record RutaItemResponse(
    Guid Id,
    string Codigo,
    string Nombre,
    string Empresa,
    string MedioTransporte,
    string ParadaOrigen,
    string ParadaDestino,
    decimal TarifaBaseMxn,
    string PrimerHorario,
    string UltimoHorario,
    int FrecuenciaMinutos,
    string DiasOperacion,
    bool Activo
);

public sealed record ParadaItemResponse(
    string ParadaId,
    string Nombre,
    double Latitud,
    double Longitud,
    int OrdenSecuencia
);
