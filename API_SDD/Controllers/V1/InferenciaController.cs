using ApiSDD.Dtos.Intelligence;
using ApiSDD.Services.NoSql;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiSDD.Controllers.V1;

/// <summary>
/// Endpoints de Machine Learning e Inteligencia Artificial Generativa Grounded (EP-IA-01..03, Spec 36 y 37).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
public sealed class InferenciaController : ControllerBase
{
    private readonly IIntelligenceService _intelligence;

    public InferenciaController(IIntelligenceService intelligence)
    {
        _intelligence = intelligence ?? throw new ArgumentNullException(nameof(intelligence));
    }

    // POST /api/v1/telemetria/inferencia (Spec 36)
    [HttpPost("telemetria/inferencia")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> InferenciaTelemetria(
        [FromBody] TelemetriaInferenciaRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Muestras is null || request.Muestras.Count == 0)
        {
            return BadRequest(new { error = "El lote de muestras inerciales no puede estar vacío.", status = 400 });
        }

        var response = await _intelligence.InferTelemetriaAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    // POST /api/v1/recomendaciones (EP-IA-01)
    [HttpPost("recomendaciones")]
    [AllowAnonymous]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> GenerarRecomendacion(
        [FromBody] RecomendacionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Origen)
            && string.IsNullOrWhiteSpace(request.Destino)
            && string.IsNullOrWhiteSpace(request.PreguntaLibre))
        {
            return BadRequest(new { error = "Debe proporcionar al menos origen/destino o una consulta de movilidad.", status = 400 });
        }

        var response = await _intelligence.GenerateRecomendacionAsync(User, request, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    // GET /api/v1/recomendaciones/historial (EP-IA-02)
    [HttpGet("recomendaciones/historial")]
    [Authorize]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> Historial(CancellationToken cancellationToken)
    {
        var items = await _intelligence.ListHistorialRecomendacionesAsync(User, cancellationToken).ConfigureAwait(false);
        return Ok(items);
    }

    // POST /api/v1/recomendaciones/{id}/retroalimentacion (EP-IA-03)
    [HttpPost("recomendaciones/{id}/retroalimentacion")]
    [Authorize]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Retroalimentacion(
        [FromRoute] string id,
        [FromBody] RetroalimentacionRecomendacionRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _intelligence.SubmitRetroalimentacionAsync(User, id, request, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }
}

public sealed record TelemetriaInferenciaRequest(
    string ViajeId,
    string RutaId,
    int IntervaloMuestreoMs,
    IReadOnlyList<MuestraInercialDto> Muestras);

public sealed record MuestraInercialDto(
    int TimestampRelativoMs,
    double AccelX,
    double AccelY,
    double AccelZ);

public sealed record TelemetriaInferenciaResponse(
    string InferenciaId,
    string ViajeId,
    string Clasificacion,
    double Confianza,
    int Severidad,
    int TotalMuestrasProcesadas,
    int ProcesadoEnMs);

public sealed record RecomendacionRequest(
    string? Origen,
    string? Destino,
    string? Preferencia,
    string? HoraSalidaDeseada,
    string? PreguntaLibre);

public sealed record RecomendacionResponse(
    string RecomendacionId,
    IReadOnlyList<RutaSugeridaDto> RutasSugeridas,
    string Explicacion,
    string FuenteDatos,
    DateTime GeneradoEn);

public sealed record RutaSugeridaDto(
    string RutaId,
    string Codigo,
    string Nombre,
    decimal TarifaEstimadaMxn,
    int TiempoEstimadoMinutos,
    string ParadaAbordaje,
    string ParadaDescenso);
