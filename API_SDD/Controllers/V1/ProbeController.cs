using ApiSDD.Common;
using Asp.Versioning;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiSDD.Controllers.V1;

/// <summary>
/// Sondas de errores solo no-prod + EnableProviderStates.
/// conflict/invalid extienden la tabla 8 de mapeos (ver Memoria.md fase nv-01).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[EnableRateLimiting("Global")]
public sealed class ProbeController : ControllerBase
{
    private readonly IHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public ProbeController(IHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    private bool Enabled => !_environment.IsProduction()
        && _configuration.GetValue("EnableProviderStates", false);

    [HttpGet("timeout")]
    public async Task<IActionResult> Timeout(CancellationToken cancellationToken)
    {
        if (!Enabled)
        {
            return NotFound();
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(1));
        await Task.Delay(TimeSpan.FromSeconds(5), timeout.Token).ConfigureAwait(false);
        return Ok();
    }

    [HttpGet("error")]
    public IActionResult Error()
    {
        if (!Enabled)
        {
            return NotFound();
        }

        throw new InvalidOperationException("Probe failure.");
    }

    [HttpGet("forbidden")]
    public IActionResult Forbidden()
    {
        if (!Enabled)
        {
            return NotFound();
        }

        throw new ForbiddenException();
    }

    [HttpGet("conflict")]
    public IActionResult ConflictProbe()
    {
        if (!Enabled)
        {
            return NotFound();
        }

        throw new ConflictException();
    }

    [HttpGet("invalid")]
    public IActionResult Invalid()
    {
        if (!Enabled)
        {
            return NotFound();
        }

        throw new RequestValidationException();
    }
}
