using System.Diagnostics.CodeAnalysis;

namespace ApiSDD.Services.Moderation;

/// <summary>
/// Resultado estructurado de moderación de texto, imagen o video previo al almacenamiento (Req. 16).
/// </summary>
public sealed record ModerationDecision(
    bool Aprobado,
    string Motivo,
    double PuntuacionSeguridad,
    IReadOnlyList<string> InfraccionesDetectadas,
    string Proveedor);

/// <summary>
/// Servicio de moderación obligatorio antes de persistir contenido de usuario o subir media a Blob CDN.
/// </summary>
public interface IContentModerationService
{
    Task<ModerationDecision> ModerateTextAsync(string? texto, string? tipoContenido, CancellationToken ct);

    [SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "Contrato JSON deModeracionRequest.")]
    Task<ModerationDecision> ModerateUrlAndTextAsync(string? texto, string? imagenUrl, string? tipoContenido, CancellationToken ct);

    Task<ModerationDecision> ModerateUploadedMediaAsync(
        byte[] contentBytes,
        string fileName,
        string contentType,
        byte[]? previewFrameBytes,
        string? metadataText,
        CancellationToken ct);
}
