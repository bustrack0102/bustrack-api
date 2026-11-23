using System.Globalization;
using System.Text.RegularExpressions;
using ApiSDD.Common;
using ApiSDD.Services.Moderation;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ApiSDD.Services.Storage;

/// <summary>
/// Sube media únicamente tras aprobar IContentModerationService y publica URL HTTPS con Cache-Control CDN (1 año inmutable).
/// </summary>
internal sealed partial class BlobMediaService : IBlobMediaService
{
    private const string DefaultCacheControl = "public, max-age=31536000, immutable";
    private const string DefaultContainer = "media";

    private static readonly Action<ILogger, string, Exception?> LogBlobFallback =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(3201, "BlobUploadFallback"),
            "Azure Blob Storage no disponible ({Reason}); devolviendo URL controlada de fallback.");

    private readonly IContentModerationService _moderation;
    private readonly IConfiguration _configuration;
    private readonly TimeProvider _time;
    private readonly ILogger<BlobMediaService> _logger;

    public BlobMediaService(
        IContentModerationService moderation,
        IConfiguration configuration,
        TimeProvider time,
        ILogger<BlobMediaService> logger)
    {
        _moderation = moderation ?? throw new ArgumentNullException(nameof(moderation));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _time = time ?? throw new ArgumentNullException(nameof(time));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [GeneratedRegex(@"[^a-z0-9\-]")]
    private static partial Regex NonSlugCharsRegex();

    public async Task<MediaUploadResult> ModerateAndUploadAsync(
        byte[] contentBytes,
        string fileName,
        string contentType,
        string categoria,
        byte[]? previewFrameBytes,
        string? notas,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(contentBytes);

        // Regla constitucional / Req. 16: moderación OBLIGATORIA antes de almacenar bytes en nube.
        var decision = await _moderation.ModerateUploadedMediaAsync(
            contentBytes,
            fileName,
            contentType,
            previewFrameBytes,
            notas,
            ct).ConfigureAwait(false);

        if (!decision.Aprobado)
        {
            return new MediaUploadResult(false, null, null, contentType, contentBytes.Length, decision.Proveedor, decision);
        }

        var normalizedMime = DetectCanonicalMime(contentBytes, contentType);
        var safeCategory = SanitizeSegment(string.IsNullOrWhiteSpace(categoria) ? "general" : categoria);
        var ext = ResolveExtension(normalizedMime);
        var now = _time.GetUtcNow();
        var blobName = string.Create(
            CultureInfo.InvariantCulture,
            $"{safeCategory}/{now:yyyy}/{now:MM}/{Guid.NewGuid():N}{ext}");

        var blobCs = ConfigurationSanitizer.CleanSecret(_configuration["AzureStorage:ConnectionString"]);
        if (ConfigurationSanitizer.HasUsableValue(blobCs))
        {
            var uploadedUrl = await TryUploadToAzureBlobAsync(
                blobCs,
                blobName,
                contentBytes,
                normalizedMime,
                ct).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(uploadedUrl))
            {
                return new MediaUploadResult(
                    true,
                    uploadedUrl,
                    DefaultCacheControl,
                    normalizedMime,
                    contentBytes.Length,
                    "AzureBlobCdn",
                    decision);
            }
        }

        var localCdn = BuildHttpsUrl("media.bustrack.local", blobName);
        return new MediaUploadResult(
            true,
            localCdn,
            DefaultCacheControl,
            normalizedMime,
            contentBytes.Length,
            "EphemeralHttpsPlaceholder",
            decision);
    }

    private async Task<string?> TryUploadToAzureBlobAsync(
        string connectionString,
        string blobName,
        byte[] contentBytes,
        string contentType,
        CancellationToken ct)
    {
        try
        {
            var configuredContainer = ConfigurationSanitizer.CleanSecret(_configuration["AzureStorage:MediaContainer"]);
            var containerName = string.IsNullOrWhiteSpace(configuredContainer) ? DefaultContainer : configuredContainer;
            var service = new BlobServiceClient(connectionString);
            var container = service.GetBlobContainerClient(containerName);
            await container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct).ConfigureAwait(false);

            var blob = container.GetBlobClient(blobName);
            using var ms = new MemoryStream(contentBytes, writable: false);
            var headers = new BlobHttpHeaders
            {
                ContentType = contentType,
                CacheControl = DefaultCacheControl,
            };
            await blob.UploadAsync(ms, new BlobUploadOptions { HttpHeaders = headers }, ct).ConfigureAwait(false);

            var cdnBase = ConfigurationSanitizer.CleanSecret(_configuration["AzureStorage:CdnBaseUrl"]);
            if (!string.IsNullOrWhiteSpace(cdnBase)
                && Uri.TryCreate(cdnBase, UriKind.Absolute, out var cdnUri)
                && cdnUri.Scheme == Uri.UriSchemeHttps)
            {
                return new Uri(cdnUri, $"{containerName}/{blobName}").AbsoluteUri;
            }

            return blob.Uri.AbsoluteUri;
        }
        catch (RequestFailedException ex)
        {
            LogBlobFallback(_logger, ex.ErrorCode ?? ex.Status.ToString(CultureInfo.InvariantCulture), ex);
            return null;
        }
    }

    private static string SanitizeSegment(string input)
    {
        var lower = input.Trim().ToLowerInvariant();
        var cleaned = NonSlugCharsRegex().Replace(lower, "-").Trim('-');
        return string.IsNullOrEmpty(cleaned) ? "general" : cleaned;
    }

    private static string DetectCanonicalMime(byte[] bytes, string declaredContentType)
    {
        if (IsPng(bytes)) return "image/png";
        if (IsJpeg(bytes)) return "image/jpeg";
        if (IsWebp(bytes)) return "image/webp";
        if (IsMp4(bytes)) return "video/mp4";
        if (IsWebm(bytes)) return "video/webm";

        return string.IsNullOrWhiteSpace(declaredContentType)
            ? "application/octet-stream"
            : declaredContentType.Trim().ToLowerInvariant();
    }

    private static bool IsPng(byte[] bytes) =>
        bytes.Length >= 8 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47;

    private static bool IsJpeg(byte[] bytes) =>
        bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;

    private static bool IsWebp(byte[] bytes) =>
        bytes.Length >= 12 && bytes[0] == (byte)'R' && bytes[1] == (byte)'I' && bytes[2] == (byte)'F' && bytes[3] == (byte)'F'
        && bytes[8] == (byte)'W' && bytes[9] == (byte)'E' && bytes[10] == (byte)'B' && bytes[11] == (byte)'P';

    private static bool IsMp4(byte[] bytes) =>
        bytes.Length >= 8 && bytes[4] == (byte)'f' && bytes[5] == (byte)'t' && bytes[6] == (byte)'y' && bytes[7] == (byte)'p';

    private static bool IsWebm(byte[] bytes) =>
        bytes.Length >= 4 && bytes[0] == 0x1A && bytes[1] == 0x45 && bytes[2] == 0xDF && bytes[3] == 0xA3;

    private static string ResolveExtension(string contentType) =>
        (contentType ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "image/png" => ".png",
            "image/webp" => ".webp",
            "video/webm" => ".webm",
            "video/mp4" or "video/quicktime" => ".mp4",
            _ => ".jpg",
        };

    private static string BuildHttpsUrl(string host, string path)
    {
        var builder = new UriBuilder { Scheme = Uri.UriSchemeHttps, Host = host, Path = path.TrimStart('/') };
        return builder.Uri.AbsoluteUri;
    }
}
