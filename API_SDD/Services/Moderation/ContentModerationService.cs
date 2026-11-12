using System.Text.RegularExpressions;
using Azure;
using Azure.AI.ContentSafety;

namespace ApiSDD.Services.Moderation;

/// <summary>
/// Moderación híbrida determinista + Azure AI Content Safety F0 (texto, imagen y video) antes de persistir (Req. 16).
/// </summary>
internal sealed partial class ContentModerationService : IContentModerationService
{
    private const int MaxImageBytes = 5 * 1024 * 1024;
    private const int MaxVideoBytes = 25 * 1024 * 1024;
    private const int SeverityRejectThreshold = 2;

    private static readonly string[] PalabrasInapropiadas =
    [
        "groseria", "insulto", "estafa", "fraude", "odio", "violencia",
        "discriminacion", "arma", "droga", "porno", "narcotrafico", "secuestro",
    ];

    private static readonly Action<ILogger, string, Exception?> LogCloudFallback =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(3101, "ContentSafetyFallback"),
            "Content Safety externo no disponible ({Reason}); se mantiene validación local estricta.");

    private readonly IConfiguration _configuration;
    private readonly ILogger<ContentModerationService> _logger;

    public ContentModerationService(
        IConfiguration configuration,
        ILogger<ContentModerationService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [GeneratedRegex(@"\b(?:ignora\s+(?:todas\s+)?(?:las\s+)?instrucciones|system\s*prompt|jailbreak|<\|im_start\|>)\b", RegexOptions.IgnoreCase)]
    private static partial Regex PromptInjectionRegex();

    [GeneratedRegex(@"(?:<\s*script\b|javascript\s*:|on(?:error|load|click)\s*=|<\s*iframe\b)", RegexOptions.IgnoreCase)]
    private static partial Regex XssInjectionRegex();

    [GeneratedRegex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b")]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"\b(?:\+?52[-.\s]?)?(?:\d{2,3}[-.\s]?){2}\d{4}\b")]
    private static partial Regex PhoneRegex();

    public Task<ModerationDecision> ModerateTextAsync(string? texto, string? tipoContenido, CancellationToken ct) =>
        ModerateUrlAndTextAsync(texto, null, tipoContenido, ct);

    public async Task<ModerationDecision> ModerateUrlAndTextAsync(
        string? texto,
        string? imagenUrl,
        string? tipoContenido,
        CancellationToken ct)
    {
        var infracciones = new List<string>();
        var raw = texto ?? string.Empty;

        EvaluateDeterministicText(raw, infracciones);
        if (!string.IsNullOrWhiteSpace(tipoContenido))
        {
            EvaluateDeterministicText(tipoContenido, infracciones);
        }

        ValidateOptionalUrl(imagenUrl, infracciones);

        var provider = "PolicyEngine";
        if (infracciones.Count == 0 && !string.IsNullOrWhiteSpace(raw))
        {
            var client = TryCreateClient();
            if (client is not null)
            {
                provider = "AzureContentSafety+PolicyEngine";
                await AnalyzeCloudTextAsync(client, raw, infracciones, ct).ConfigureAwait(false);
            }
        }

        return BuildDecision(infracciones, provider);
    }

    public async Task<ModerationDecision> ModerateUploadedMediaAsync(
        byte[] contentBytes,
        string fileName,
        string contentType,
        byte[]? previewFrameBytes,
        string? metadataText,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(contentBytes);

        var infracciones = new List<string>();
        if (!string.IsNullOrWhiteSpace(metadataText))
        {
            EvaluateDeterministicText(metadataText, infracciones);
        }

        if (!string.IsNullOrWhiteSpace(fileName))
        {
            EvaluateDeterministicText(fileName, infracciones);
        }

        var kind = ClassifyAndValidateBinary(contentBytes, contentType, infracciones);
        var provider = kind == MediaBinaryKind.Video ? "VideoGuardrail+PolicyEngine" : "ImageGuardrail+PolicyEngine";

        if (infracciones.Count == 0)
        {
            var client = TryCreateClient();
            if (client is not null)
            {
                provider = "AzureContentSafety+" + provider;
                byte[]? bytesToScan = kind == MediaBinaryKind.Image
                    ? contentBytes
                    : ValidatePreviewFrame(previewFrameBytes, infracciones);

                if (bytesToScan is not null && infracciones.Count == 0)
                {
                    await AnalyzeCloudImageAsync(client, bytesToScan, infracciones, ct).ConfigureAwait(false);
                }
            }
        }

        return BuildDecision(infracciones, provider);
    }

    private static void EvaluateDeterministicText(string texto, List<string> infracciones)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return;
        }

        if (PromptInjectionRegex().IsMatch(texto))
        {
            infracciones.Add("Intento de manipulación de instrucciones (Prompt Injection).");
        }

        if (XssInjectionRegex().IsMatch(texto))
        {
            infracciones.Add("Patrón activo de script o marcado peligroso no permitido.");
        }

        foreach (var palabra in PalabrasInapropiadas.Where(p => texto.Contains(p, StringComparison.OrdinalIgnoreCase)))
        {
            infracciones.Add($"Uso de término inapropiado: '{palabra}'.");
        }

        if (EmailRegex().IsMatch(texto) || PhoneRegex().IsMatch(texto))
        {
            infracciones.Add("Presencia de datos personales sensibles (teléfono o correo electrónico).");
        }
    }

    private static void ValidateOptionalUrl(string? imagenUrl, List<string> infracciones)
    {
        if (string.IsNullOrWhiteSpace(imagenUrl))
        {
            return;
        }

        if (!Uri.TryCreate(imagenUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            infracciones.Add("URL de imagen con protocolo no seguro o formato inválido.");
        }
    }

    private static MediaBinaryKind ClassifyAndValidateBinary(
        byte[] bytes,
        string? contentType,
        List<string> infracciones)
    {
        if (bytes.Length < 12)
        {
            infracciones.Add("Archivo vacío o corrupto (longitud insuficiente).");
            return MediaBinaryKind.Unknown;
        }

        if (LooksLikeScriptOrExecutable(bytes))
        {
            infracciones.Add("Encabezado binario o script potencialmente malicioso bloqueado.");
            return MediaBinaryKind.Unknown;
        }

        var ctNormalized = (contentType ?? string.Empty).Trim().ToLowerInvariant();
        if (ctNormalized.StartsWith("video/", StringComparison.Ordinal))
        {
            ValidateVideo(bytes, ctNormalized, infracciones);
            return MediaBinaryKind.Video;
        }

        if (ctNormalized.StartsWith("image/", StringComparison.Ordinal))
        {
            ValidateImage(bytes, ctNormalized, infracciones);
            return MediaBinaryKind.Image;
        }

        infracciones.Add("Tipo MIME no soportado; solo imágenes (JPEG/PNG/WebP) o videos (MP4/WebM).");
        return MediaBinaryKind.Unknown;
    }

    private static void ValidateImage(byte[] bytes, string mime, List<string> infracciones)
    {
        if (bytes.Length > MaxImageBytes)
        {
            infracciones.Add("La imagen supera el límite máximo permitido de 5 MB.");
            return;
        }

        if (!IsSupportedImageMagic(bytes) || mime == "image/svg+xml")
        {
            infracciones.Add("Firma binaria de imagen inválida (se requiere JPEG, PNG o WebP real).");
        }
    }

    private static void ValidateVideo(byte[] bytes, string mime, List<string> infracciones)
    {
        if (bytes.Length > MaxVideoBytes)
        {
            infracciones.Add("El video supera el límite máximo permitido de 25 MB.");
            return;
        }

        if (mime is not ("video/mp4" or "video/webm" or "video/quicktime"))
        {
            infracciones.Add("Contenedor de video no soportado; se requiere MP4 o WebM.");
            return;
        }

        var isMp4 = bytes.Length >= 8 && bytes[4] == (byte)'f' && bytes[5] == (byte)'t' && bytes[6] == (byte)'y' && bytes[7] == (byte)'p';
        var isWebM = bytes[0] == 0x1A && bytes[1] == 0x45 && bytes[2] == 0xDF && bytes[3] == 0xA3;
        if (!isMp4 && !isWebM)
        {
            infracciones.Add("Firma binaria de video adulterada o contenedor inválido.");
        }
    }

    private static byte[]? ValidatePreviewFrame(byte[]? frameBytes, List<string> infracciones)
    {
        if (frameBytes is null || frameBytes.Length == 0)
        {
            return null;
        }

        if (!IsSupportedImageMagic(frameBytes) || frameBytes.Length > MaxImageBytes)
        {
            infracciones.Add("El fotograma clave adjunto del video no corresponde a una imagen válida.");
            return null;
        }

        return frameBytes;
    }

    private static bool IsSupportedImageMagic(byte[] bytes)
    {
        if (bytes.Length < 12)
        {
            return false;
        }

        var isJpeg = bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;
        var isPng = bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47;
        var isWebP = bytes[0] == (byte)'R' && bytes[1] == (byte)'I' && bytes[2] == (byte)'F' && bytes[3] == (byte)'F'
                     && bytes[8] == (byte)'W' && bytes[9] == (byte)'E' && bytes[10] == (byte)'B' && bytes[11] == (byte)'P';
        return isJpeg || isPng || isWebP;
    }

    private static bool LooksLikeScriptOrExecutable(byte[] bytes)
    {
        var isPe = bytes[0] == (byte)'M' && bytes[1] == (byte)'Z';
        var isElf = bytes[0] == 0x7F && bytes[1] == (byte)'E' && bytes[2] == (byte)'L' && bytes[3] == (byte)'F';
        var head = System.Text.Encoding.ASCII.GetString(bytes, 0, Math.Min(bytes.Length, 64)).TrimStart();
        var isMarkup = head.StartsWith("<script", StringComparison.OrdinalIgnoreCase)
                       || head.StartsWith("<svg", StringComparison.OrdinalIgnoreCase)
                       || head.StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase)
                       || head.StartsWith("<?php", StringComparison.OrdinalIgnoreCase);
        return isPe || isElf || isMarkup;
    }

    private ContentSafetyClient? TryCreateClient()
    {
        var endpoint = _configuration["ContentSafety:Endpoint"];
        var key = _configuration["ContentSafety:Key"];
        if (string.IsNullOrWhiteSpace(endpoint)
            || string.IsNullOrWhiteSpace(key)
            || endpoint.Contains("REEMPLAZAR", StringComparison.OrdinalIgnoreCase)
            || key.Contains("REEMPLAZAR", StringComparison.OrdinalIgnoreCase)
            || !Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
        {
            return null;
        }

        return new ContentSafetyClient(uri, new AzureKeyCredential(key));
    }

    private async Task AnalyzeCloudTextAsync(
        ContentSafetyClient client,
        string texto,
        List<string> infracciones,
        CancellationToken ct)
    {
        try
        {
            var response = await client.AnalyzeTextAsync(new AnalyzeTextOptions(texto), ct).ConfigureAwait(false);
            foreach (var item in response.Value.CategoriesAnalysis)
            {
                if (item.Severity >= SeverityRejectThreshold)
                {
                    infracciones.Add($"Moderación Cloud rechazó categoría {item.Category} (severidad {item.Severity}).");
                }
            }
        }
        catch (RequestFailedException ex)
        {
            LogCloudFallback(_logger, ex.Status.ToString(System.Globalization.CultureInfo.InvariantCulture), ex);
        }
        catch (HttpRequestException ex)
        {
            LogCloudFallback(_logger, ex.Message, ex);
        }
    }

    private async Task AnalyzeCloudImageAsync(
        ContentSafetyClient client,
        byte[] imageBytes,
        List<string> infracciones,
        CancellationToken ct)
    {
        try
        {
            var data = new ContentSafetyImageData(BinaryData.FromBytes(imageBytes));
            var response = await client.AnalyzeImageAsync(new AnalyzeImageOptions(data), ct).ConfigureAwait(false);
            foreach (var item in response.Value.CategoriesAnalysis)
            {
                if (item.Severity >= SeverityRejectThreshold)
                {
                    infracciones.Add($"Moderación visual Cloud rechazó categoría {item.Category} (severidad {item.Severity}).");
                }
            }
        }
        catch (RequestFailedException ex)
        {
            LogCloudFallback(_logger, ex.Status.ToString(System.Globalization.CultureInfo.InvariantCulture), ex);
        }
        catch (HttpRequestException ex)
        {
            LogCloudFallback(_logger, ex.Message, ex);
        }
    }

    private static ModerationDecision BuildDecision(List<string> infracciones, string provider)
    {
        var aprobado = infracciones.Count == 0;
        var puntuacion = aprobado ? 0.98 : 0.20;
        var motivo = aprobado
            ? "Contenido validado como seguro y respetuoso."
            : "Contenido no permitido por políticas comunitarias.";
        return new ModerationDecision(aprobado, motivo, puntuacion, infracciones, provider);
    }

    private enum MediaBinaryKind
    {
        Unknown,
        Image,
        Video,
    }
}
