using ApiSDD.Dtos;
using ApiSDD.Security;
using ApiSDD.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

// Controller name follows the versioned resource path; Sonar S101 cannot apply.
// Login+logout form one cohesive auth resource slice; Sonar S6960 split does not apply.
#pragma warning disable S101, S6960

namespace ApiSDD.Controllers.V1;

/// <summary>nv-05 login + nv-06 logout slices. Errors flow to the middleware.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    private readonly ITokenRevocationService _revocation;

    public AuthController(IAuthService service, ITokenRevocationService revocation)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _revocation = revocation ?? throw new ArgumentNullException(nameof(revocation));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("Login")]
    public async Task<ActionResult<LoginResponseDto>> Login(
        [FromBody] LoginRequestDto dto,
        CancellationToken cancellationToken)
    {
        var response = await _service.LoginAsync(dto, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("logout")]
    [Authorize]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await _revocation.RevokeCurrentAsync(User, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
