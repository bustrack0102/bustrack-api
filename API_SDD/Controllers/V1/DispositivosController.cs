using ApiSDD.Common;
using ApiSDD.Dtos.Operational;
using ApiSDD.Services.Notifications;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiSDD.Controllers.V1;

/// <summary>
/// Registro de dispositivos móviles para notificaciones Push FCM (Req. 16).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/dispositivos")]
public sealed class DispositivosController : ControllerBase
{
    private readonly IPushNotificationService _push;

    public DispositivosController(IPushNotificationService push)
    {
        _push = push ?? throw new ArgumentNullException(nameof(push));
    }

    [HttpGet("mios")]
    [Authorize]
    [EnableRateLimiting("Global")]
    public async Task<IActionResult> ListMyDevices(CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var items = await _push.ListMyDevicesAsync(userId, cancellationToken).ConfigureAwait(false);
        return Ok(items);
    }

    [HttpPost]
    [Authorize]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Register(
        [FromBody] DispositivoRegisterRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var item = await _push.RegisterDeviceAsync(userId, request, cancellationToken).ConfigureAwait(false);
        return Created(new Uri($"/api/v1/dispositivos/{item.Id}", UriKind.Relative), item);
    }

    [HttpDelete("{id}")]
    [Authorize]
    [EnableRateLimiting("ConcurrentWrites")]
    public async Task<IActionResult> Unregister([FromRoute] string id, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var removed = await _push.UnregisterDeviceAsync(userId, id, cancellationToken).ConfigureAwait(false);
        return removed ? NoContent() : NotFound(new { error = "Dispositivo no encontrado.", status = 404 });
    }
}
