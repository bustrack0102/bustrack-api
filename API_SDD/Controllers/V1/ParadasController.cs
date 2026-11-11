using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiSDD.Controllers.V1;

/// <summary>
/// Controlador de paradas de autobús con geolocalización y proximidad (Spec 20 y 32).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/paradas")]
public sealed class ParadasController : ControllerBase
{
    private static readonly IReadOnlyList<ParadaDetalleDto> ParadasHidalgo =
    [
        new("PAR-01", "Central de Autobuses de Pachuca", "Pachuca de Soto", 20.1012, -98.7523, true, 8),
        new("PAR-02", "Plaza Juárez Centro Pachuca", "Pachuca de Soto", 20.1245, -98.7321, true, 12),
        new("PAR-03", "Puente Actopan Troncal", "Actopan", 20.2687, -98.9431, true, 6),
        new("PAR-04", "Central de Autobuses de Tula", "Tula de Allende", 20.0512, -99.3421, true, 10),
        new("PAR-05", "Crucero Cruz Azul - San Marcos", "Tula de Allende", 20.0345, -99.2891, true, 4),
        new("PAR-06", "Jardín Municipal Tepeji del Río", "Tepeji del Río", 19.9056, -99.3412, true, 5),
        new("PAR-07", "Monumento al Minero Real del Monte", "Mineral del Monte", 20.1412, -98.6712, true, 3)
    ];

    // GET /api/v1/paradas
    [HttpGet]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public IActionResult List([FromQuery] string? municipio = null)
    {
        var result = ParadasHidalgo.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(municipio))
        {
            var m = municipio.Trim();
            result = result.Where(p => p.Municipio.Contains(m, StringComparison.OrdinalIgnoreCase));
        }

        Response.Headers.Append("Cache-Control", "public, max-age=120");
        return Ok(result.ToList());
    }

    // GET /api/v1/paradas/cercanas?lat=20.05&lon=-99.34&radioKm=5
    [HttpGet("cercanas")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public IActionResult GetCercanas(
        [FromQuery] double lat,
        [FromQuery] double lon,
        [FromQuery] double radioKm = 10.0)
    {
        var cercanas = ParadasHidalgo
            .Select(p => new
            {
                Parada = p,
                DistanciaKm = CalcularDistanciaHaversine(lat, lon, p.Latitud, p.Longitud)
            })
            .Where(x => x.DistanciaKm <= radioKm)
            .OrderBy(x => x.DistanciaKm)
            .Select(x => new
            {
                x.Parada.Id,
                x.Parada.Nombre,
                x.Parada.Municipio,
                x.Parada.Latitud,
                x.Parada.Longitud,
                x.Parada.RutasDisponibles,
                DistanciaAproximadaKm = Math.Round(x.DistanciaKm, 2)
            })
            .ToList();

        return Ok(cercanas);
    }

    private static double CalcularDistanciaHaversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double r = 6371.0; // Radio de la Tierra en km
        var dLat = (lat2 - lat1) * (Math.PI / 180.0);
        var dLon = (lon2 - lon1) * (Math.PI / 180.0);
        var a = Math.Sin(dLat / 2.0) * Math.Sin(dLat / 2.0) +
                Math.Cos(lat1 * (Math.PI / 180.0)) * Math.Cos(lat2 * (Math.PI / 180.0)) *
                Math.Sin(dLon / 2.0) * Math.Sin(dLon / 2.0);
        var c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));
        return r * c;
    }
}

public sealed record ParadaDetalleDto(
    string Id,
    string Nombre,
    string Municipio,
    double Latitud,
    double Longitud,
    bool Techada,
    int RutasDisponibles
);
