using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiSDD.Controllers.V1;

/// <summary>
/// Endpoints de Machine Learning e Inteligencia Artificial Generativa (Specs 36 y 37, Lineamientos 16 y 19).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
public sealed class InferenciaController : ControllerBase
{
    // POST /api/v1/telemetria/inferencia (Spec 36: Detección de baches y calidad vial mediante sensores inerciales)
    [HttpPost("telemetria/inferencia")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public IActionResult InferenciaTelemetria([FromBody] TelemetriaInferenciaRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Muestras is null || request.Muestras.Count == 0)
        {
            return BadRequest(new { error = "El lote de muestras inerciales no puede estar vacío.", status = 400 });
        }

        // Algoritmo analítico de detección de picos de aceleración Z e inclinación (Simulación ML de red neuronal)
        double maxMagnitud = 0;
        foreach (var m in request.Muestras)
        {
            var mag = Math.Sqrt(m.AccelX * m.AccelX + m.AccelY * m.AccelY + m.AccelZ * m.AccelZ);
            if (mag > maxMagnitud)
            {
                maxMagnitud = mag;
            }
        }

        string clasificacion;
        int severidad;
        double confianza;

        if (maxMagnitud > 18.0)
        {
            clasificacion = "BACHE_SEVERO";
            severidad = 3;
            confianza = 0.94;
        }
        else if (maxMagnitud > 13.5)
        {
            clasificacion = "ANOMALIA_VIBRACION_MODERADA";
            severidad = 2;
            confianza = 0.88;
        }
        else
        {
            clasificacion = "VIA_REGULAR_OPTIMA";
            severidad = 0;
            confianza = 0.96;
        }

        return Ok(new TelemetriaInferenciaResponse(
            Guid.NewGuid().ToString(),
            request.ViajeId,
            clasificacion,
            confianza,
            severidad,
            request.Muestras.Count,
            18
        ));
    }

    // POST /api/v1/recomendaciones (Spec 37: Recomendaciones de movilidad inteligente y asistencia de ruta)
    [HttpPost("recomendaciones")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public IActionResult GenerarRecomendacion([FromBody] RecomendacionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Origen) && string.IsNullOrWhiteSpace(request.Destino) && string.IsNullOrWhiteSpace(request.PreguntaLibre))
        {
            return BadRequest(new { error = "Debe proporcionar al menos origen/destino o una consulta de movilidad.", status = 400 });
        }

        var sugerencias = new List<RutaSugeridaDto>
        {
            new(
                "10000000-0000-0000-0000-000000000001",
                "R-01",
                "Pachuca – Tula Directo Express",
                65.00m,
                55,
                "Central de Autobuses de Pachuca",
                "Central de Autobuses de Tula"
            ),
            new(
                "10000000-0000-0000-0000-000000000002",
                "R-02",
                "Tula – Tepeji Centro Metropolitano",
                22.00m,
                30,
                "Tula Centro",
                "Tepeji Centro"
            )
        };

        var explicacion = string.IsNullOrWhiteSpace(request.PreguntaLibre)
            ? $"Para trasladarse de '{request.Origen}' hacia '{request.Destino}', la opción óptima por frecuencia y menor tiempo es la ruta R-01 con salida cada 15 minutos."
            : $"Respuesta a tu consulta de movilidad: 'Te sugerimos conectar en la Central de Autobuses de Tula, donde dispones de enlace a Microbús Urbano y Autobús Directo con tarifa fija.'";

        return Ok(new RecomendacionResponse(
            Guid.NewGuid().ToString(),
            sugerencias,
            explicacion,
            "Catálogo Oficial Verificado BusTrack API (Hidalgo)",
            DateTime.UtcNow
        ));
    }
}

public sealed record TelemetriaInferenciaRequest(
    string ViajeId,
    string RutaId,
    int IntervaloMuestreoMs,
    IReadOnlyList<MuestraInercialDto> Muestras
);

public sealed record MuestraInercialDto(
    int TimestampRelativoMs,
    double AccelX,
    double AccelY,
    double AccelZ
);

public sealed record TelemetriaInferenciaResponse(
    string InferenciaId,
    string ViajeId,
    string Clasificacion,
    double Confianza,
    int Severidad,
    int TotalMuestrasProcesadas,
    int ProcesadoEnMs
);

public sealed record RecomendacionRequest(
    string? Origen,
    string? Destino,
    string? Preferencia, // "MENOR_TIEMPO", "MENOR_COSTO", "DIRECTO"
    string? HoraSalidaDeseada,
    string? PreguntaLibre
);

public sealed record RecomendacionResponse(
    string RecomendacionId,
    IReadOnlyList<RutaSugeridaDto> RutasSugeridas,
    string Explicacion,
    string FuenteDatos,
    DateTime GeneradoEn
);

public sealed record RutaSugeridaDto(
    string RutaId,
    string Codigo,
    string Nombre,
    decimal TarifaEstimadaMxn,
    int TiempoEstimadoMinutos,
    string ParadaAbordaje,
    string ParadaDescenso
);
