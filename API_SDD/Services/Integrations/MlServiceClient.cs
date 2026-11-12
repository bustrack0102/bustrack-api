using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ApiSDD.Common;
using ApiSDD.Dtos.Intelligence;

namespace ApiSDD.Services.Integrations;

/// <summary>
/// Cliente tipado resiliente hacia el microservicio interno bustrack-ml (POST /inference).
/// </summary>
public interface IMlServiceClient
{
    Task<MlInferenceUpstreamResponse> DetectAsync(
        string viajeId,
        string rutaId,
        int intervaloMuestreoMs,
        bool consentimiento,
        IReadOnlyList<SensorMuestraDto> muestras,
        CancellationToken ct);
}

internal sealed class MlServiceClient : IMlServiceClient
{
    private static readonly Action<ILogger, string, Exception?> LogMlFallback =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(3401, "MlUpstreamFallback"),
            "Servicio interno bustrack-ml no accesible ({Reason}); aplicando inferencia inercial local.");

    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MlServiceClient> _logger;

    public MlServiceClient(
        IHttpClientFactory httpFactory,
        IConfiguration configuration,
        ILogger<MlServiceClient> logger)
    {
        _httpFactory = httpFactory ?? throw new ArgumentNullException(nameof(httpFactory));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MlInferenceUpstreamResponse> DetectAsync(
        string viajeId,
        string rutaId,
        int intervaloMuestreoMs,
        bool consentimiento,
        IReadOnlyList<SensorMuestraDto> muestras,
        CancellationToken ct)
    {
        var baseUrl = ConfigurationSanitizer.CleanSecret(_configuration["Services:MlBaseUrl"]);
        if (ConfigurationSanitizer.HasUsableValue(baseUrl)
            && Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
        {
            var upstream = await TryCallRemoteMlAsync(
                baseUri,
                viajeId,
                rutaId,
                intervaloMuestreoMs,
                consentimiento,
                muestras,
                ct).ConfigureAwait(false);
            if (upstream is not null)
            {
                return upstream;
            }
        }

        return EvaluateDeterministicLocal(viajeId, rutaId, muestras);
    }

    private async Task<MlInferenceUpstreamResponse?> TryCallRemoteMlAsync(
        Uri baseUri,
        string viajeId,
        string rutaId,
        int intervaloMuestreoMs,
        bool consentimiento,
        IReadOnlyList<SensorMuestraDto> muestras,
        CancellationToken ct)
    {
        try
        {
            var client = _httpFactory.CreateClient("MlService");
            var targetUri = new Uri(baseUri, "inference");
            using var msg = new HttpRequestMessage(HttpMethod.Post, targetUri);
            ConfigurationSanitizer.AttachInternalKeyHeader(msg, _configuration);

            msg.Content = JsonContent.Create(new MlInferenceUpstreamRequest(
                viajeId,
                rutaId,
                intervaloMuestreoMs,
                consentimiento,
                muestras));

            using var resp = await client.SendAsync(msg, ct).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                LogMlFallback(_logger, $"HTTP {(int)resp.StatusCode}", null);
                return null;
            }

            return await resp.Content.ReadFromJsonAsync<MlInferenceUpstreamResponse>(cancellationToken: ct)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            LogMlFallback(_logger, ex.Message, ex);
            return null;
        }
        catch (TaskCanceledException ex)
        {
            LogMlFallback(_logger, ex.Message, ex);
            return null;
        }
        catch (JsonException ex)
        {
            LogMlFallback(_logger, ex.Message, ex);
            return null;
        }
    }

    internal static MlInferenceUpstreamResponse EvaluateDeterministicLocal(
        string viajeId,
        string rutaId,
        IReadOnlyList<SensorMuestraDto> muestras)
    {
        var mags = muestras
            .Select(m => Math.Sqrt((m.AccelX * m.AccelX) + (m.AccelY * m.AccelY) + (m.AccelZ * m.AccelZ)))
            .ToArray();
        var mean = mags.Average();
        var peak = mags.Max();
        var variance = mags.Average(v => Math.Pow(v - mean, 2));
        var std = Math.Sqrt(variance);

        string label;
        int severity;
        double confidence;
        if (peak >= 18.0)
        {
            label = "BACHE_SEVERO";
            severity = 3;
            confidence = 0.94;
        }
        else if (peak >= 13.5 || std >= 1.8)
        {
            label = "ANOMALIA_VIBRACION_MODERADA";
            severity = 2;
            confidence = 0.88;
        }
        else if (muestras.Count >= 4 && std <= 0.12 && mean is >= 9.3 and <= 10.3)
        {
            label = "PARADA_DETECTADA";
            severity = 1;
            confidence = 0.86;
        }
        else
        {
            label = "VIA_REGULAR_OPTIMA";
            severity = 0;
            confidence = 0.95;
        }

        return new MlInferenceUpstreamResponse(
            Guid.NewGuid().ToString(),
            ComputeAnonHash(viajeId),
            rutaId,
            "1.0.0-local-fallback",
            label,
            confidence,
            severity,
            muestras.Count,
            12);
    }

    private static string ComputeAnonHash(string viajeId)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes("bustrack-salt:" + viajeId));
        return Convert.ToHexString(bytes, 0, 8).ToLowerInvariant();
    }
}
