using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiSDD.Controllers.V1;

/// <summary>Liveness de ruteo versionado + convención JSON legacy.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting("Global")]
public sealed class PingController : ControllerBase
{
    [HttpGet]
    public ActionResult<PingResponse> Get() => Ok(new PingResponse("Pong"));
}

/// <summary>Wire legacy: clave minúscula de una palabra (ver 03-api-base).</summary>
public sealed record PingResponse([property: JsonPropertyName("status")] string Status);
