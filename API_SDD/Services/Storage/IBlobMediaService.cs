using System.Diagnostics.CodeAnalysis;
using ApiSDD.Services.Moderation;

namespace ApiSDD.Services.Storage;

public sealed record MediaUploadResult(
    bool Success,
    [property: SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "Contrato JSON.")]
    string? Url,
    string? CacheControl,
    string? ContentType,
    long SizeBytes,
    string Provider,
    ModerationDecision Moderation);

/// <summary>
/// Servicio de almacenamiento de imágenes y video corto en Blob Storage + CDN con moderación obligatoria previa (Reqs. 15 y 16).
/// </summary>
public interface IBlobMediaService
{
    Task<MediaUploadResult> ModerateAndUploadAsync(
        byte[] contentBytes,
        string fileName,
        string contentType,
        string categoria,
        byte[]? previewFrameBytes,
        string? notas,
        CancellationToken ct);
}
