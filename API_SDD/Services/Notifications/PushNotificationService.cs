using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ApiSDD.Common;
using ApiSDD.Data.NoSql;
using ApiSDD.Dtos.Operational;

namespace ApiSDD.Services.Notifications;

/// <summary>
/// Cliente Push FCM HTTP v1 con firma RS256 nativa (sin SDK externo pesado) y degradación segura en tests/local.
/// </summary>
internal sealed class PushNotificationService : IPushNotificationService
{
    private static readonly Action<ILogger, string, int, Exception?> LogPushDispatch =
        LoggerMessage.Define<string, int>(
            LogLevel.Information,
            new EventId(3301, "FcmDispatch"),
            "Push enviado en modo {Modo} hacia {Count} dispositivo(s).");

    private static readonly Action<ILogger, string, Exception?> LogPushFallback =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(3302, "FcmFallback"),
            "Fallo transitorio en proveedor FCM ({Reason}); notificación degradada sin interrumpir flujo.");

    private readonly IBusTrackNoSqlStore _store;
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _configuration;
    private readonly TimeProvider _time;
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(
        IBusTrackNoSqlStore store,
        IHttpClientFactory httpFactory,
        IConfiguration configuration,
        TimeProvider time,
        ILogger<PushNotificationService> logger)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _httpFactory = httpFactory ?? throw new ArgumentNullException(nameof(httpFactory));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _time = time ?? throw new ArgumentNullException(nameof(time));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DispositivoItemDto> RegisterDeviceAsync(
        Guid userId,
        DispositivoRegisterRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        var handle = request.TokenFcm.Trim();
        if (handle.Length < 12)
        {
            throw new RequestValidationException("Identificador de dispositivo FCM inválido.");
        }

        var platform = request.Plataforma.Trim().ToLowerInvariant() switch
        {
            "ios" => "ios",
            "web" => "web",
            _ => "android",
        };

        var stableId = string.Create(
            CultureInfo.InvariantCulture,
            $"dev-{ComputeShortHash(userId + ":" + handle)}");
        var now = _time.GetUtcNow().UtcDateTime;
        var doc = new DispositivoFcmDoc
        {
            Id = stableId,
            UsuarioId = userId,
            TokenFcm = handle,
            Plataforma = platform,
            ActualizadoEnUtc = now,
        };

        await _store.UpsertDispositivoFcmAsync(doc, ct).ConfigureAwait(false);
        return MapDevice(doc);
    }

    public async Task<IReadOnlyList<DispositivoItemDto>> ListMyDevicesAsync(Guid userId, CancellationToken ct)
    {
        var items = await _store.ListDispositivosUsuarioAsync(userId, ct).ConfigureAwait(false);
        return items.Select(MapDevice).ToList();
    }

    public Task<bool> UnregisterDeviceAsync(Guid userId, string deviceId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return Task.FromResult(false);
        }

        return _store.DeleteDispositivoFcmAsync(deviceId.Trim(), userId, ct);
    }

    public async Task<PushDispatchResult> NotifyUserAsync(
        Guid userId,
        string titulo,
        string cuerpo,
        IReadOnlyDictionary<string, string>? data,
        CancellationToken ct)
    {
        var devices = await _store.ListDispositivosUsuarioAsync(userId, ct).ConfigureAwait(false);
        if (devices.Count == 0)
        {
            LogPushDispatch(_logger, "SinDispositivos", 0, null);
            return new PushDispatchResult(0, 0, "SinDispositivos");
        }

        var credsRaw = _configuration["Firebase:CredentialsJson"];
        if (string.IsNullOrWhiteSpace(credsRaw)
            || credsRaw.Contains("REEMPLAZAR", StringComparison.OrdinalIgnoreCase))
        {
            LogPushDispatch(_logger, "SimulatedLocal", devices.Count, null);
            return new PushDispatchResult(devices.Count, devices.Count, "SimulatedLocal");
        }

        var sent = await TrySendViaFcmAsync(credsRaw, devices, titulo, cuerpo, data, ct).ConfigureAwait(false);
        return new PushDispatchResult(devices.Count, sent, "FcmHttpV1");
    }

    private async Task<int> TrySendViaFcmAsync(
        string credentialsJson,
        IReadOnlyList<DispositivoFcmDoc> devices,
        string titulo,
        string cuerpo,
        IReadOnlyDictionary<string, string>? data,
        CancellationToken ct)
    {
        try
        {
            using var doc = JsonDocument.Parse(credentialsJson);
            var root = doc.RootElement;
            var projectId = root.GetProperty("project_id").GetString();
            var clientEmail = root.GetProperty("client_email").GetString();
            var privateKey = root.GetProperty("private_key").GetString();
            var oauthUriText = root.TryGetProperty("token_uri", out var uriProp) ? uriProp.GetString() : null;

            if (string.IsNullOrWhiteSpace(projectId)
                || string.IsNullOrWhiteSpace(clientEmail)
                || string.IsNullOrWhiteSpace(privateKey))
            {
                return 0;
            }

            var bearer = await ExchangeServiceAccountAssertionAsync(
                clientEmail,
                privateKey,
                oauthUriText,
                ct).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(bearer))
            {
                return 0;
            }

            var client = _httpFactory.CreateClient("Fcm");
            var endpoint = new UriBuilder(Uri.UriSchemeHttps, "fcm.googleapis.com", -1, $"/v1/projects/{projectId}/messages:send").Uri;
            var delivered = 0;

            foreach (var device in devices)
            {
                using var msg = new HttpRequestMessage(HttpMethod.Post, endpoint);
                msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
                var payload = new
                {
                    message = new
                    {
                        token = device.TokenFcm,
                        notification = new { title = titulo, body = cuerpo },
                        data = data ?? new Dictionary<string, string>(StringComparer.Ordinal),
                    },
                };
                msg.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                using var resp = await client.SendAsync(msg, ct).ConfigureAwait(false);
                if (resp.IsSuccessStatusCode)
                {
                    delivered++;
                }
            }

            LogPushDispatch(_logger, "FcmHttpV1", delivered, null);
            return delivered;
        }
        catch (JsonException ex)
        {
            LogPushFallback(_logger, ex.Message, ex);
            return 0;
        }
        catch (CryptographicException ex)
        {
            LogPushFallback(_logger, ex.Message, ex);
            return 0;
        }
        catch (HttpRequestException ex)
        {
            LogPushFallback(_logger, ex.Message, ex);
            return 0;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "S5542:Encryption algorithms should be used with secure mode and padding scheme", Justification = "Google OAuth2 RS256 Service Account JWT requiere RSASignaturePadding.Pkcs1 según RFC 7518.")]
    private async Task<string?> ExchangeServiceAccountAssertionAsync(
        string clientEmail,
        string privateKeyPem,
        string? oauthUriText,
        CancellationToken ct)
    {
        var oauthUri = Uri.TryCreate(oauthUriText, UriKind.Absolute, out var parsed)
            ? parsed
            : new UriBuilder(Uri.UriSchemeHttps, "oauth2.googleapis.com", -1, "/token").Uri;
        var scopeUri = new UriBuilder(Uri.UriSchemeHttps, "www.googleapis.com", -1, "/auth/firebase.messaging").Uri;

        var now = _time.GetUtcNow().ToUnixTimeSeconds();
        var header = Base64Url(Encoding.UTF8.GetBytes("""{"alg":"RS256","typ":"JWT"}"""));
        var claimsJson = JsonSerializer.Serialize(new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["iss"] = clientEmail,
            ["scope"] = scopeUri.AbsoluteUri,
            ["aud"] = oauthUri.AbsoluteUri,
            ["iat"] = now,
            ["exp"] = now + 3600,
        });
        var payload = Base64Url(Encoding.UTF8.GetBytes(claimsJson));
        var signingInput = Encoding.ASCII.GetBytes($"{header}.{payload}");

        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);
        var sig = Base64Url(rsa.SignData(signingInput, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
        var assertion = $"{header}.{payload}.{sig}";

        var client = _httpFactory.CreateClient("Fcm");
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
            ["assertion"] = assertion,
        });
        using var resp = await client.PostAsync(oauthUri, form, ct).ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode)
        {
            return null;
        }

        using var stream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: ct).ConfigureAwait(false);
        return json.RootElement.TryGetProperty("access_token", out var accessProp) ? accessProp.GetString() : null;
    }

    private static DispositivoItemDto MapDevice(DispositivoFcmDoc d)
    {
        var suffix = d.TokenFcm.Length <= 6 ? d.TokenFcm : d.TokenFcm[^6..];
        return new DispositivoItemDto(d.Id, d.Plataforma, suffix, d.ActualizadoEnUtc);
    }

    private static string ComputeShortHash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes, 0, 8).ToLowerInvariant();
    }

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
