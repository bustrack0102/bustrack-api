using ApiSDD.Dtos.Operational;

namespace ApiSDD.Services.Notifications;

public sealed record PushDispatchResult(
    int DispositivosDestino,
    int Enviados,
    string Modo);

/// <summary>
/// Gestión de dispositivos y envío de notificaciones Push mediante Firebase Cloud Messaging (FCM HTTP v1).
/// </summary>
public interface IPushNotificationService
{
    Task<DispositivoItemDto> RegisterDeviceAsync(Guid userId, DispositivoRegisterRequest request, CancellationToken ct);

    Task<IReadOnlyList<DispositivoItemDto>> ListMyDevicesAsync(Guid userId, CancellationToken ct);

    Task<bool> UnregisterDeviceAsync(Guid userId, string deviceId, CancellationToken ct);

    Task<PushDispatchResult> NotifyUserAsync(
        Guid userId,
        string titulo,
        string cuerpo,
        IReadOnlyDictionary<string, string>? data,
        CancellationToken ct);
}
