using System.Net.Http.Json;
using System.Text.Json;
using ApiSDD.Dtos.Intelligence;

namespace ApiSDD.Services.Integrations;

/// <summary>
/// Cliente tipado resiliente hacia bustrack-genai (POST /recommendations).
/// </summary>
public interface IGenAiServiceClient
{
    Task<GenAiUpstreamResponse?> RecommendAsync(
        GenAiUpstreamRequest request,
        CancellationToken ct);
}

internal sealed class GenAiServiceClient : IGenAiServiceClient
{
    private static readonly Action<ILogger, string, Exception?> LogGenAiFallback =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(3402, "GenAiUpstreamFallback"),
            "Servicio interno bustrack-genai no accesible ({Reason}); usando motor determinista sobre catálogo.");

    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GenAiServiceClient> _logger;

    public GenAiServiceClient(
        IHttpClientFactory httpFactory,
        IConfiguration configuration,
        ILogger<GenAiServiceClient> logger)
    {
        _httpFactory = httpFactory ?? throw new ArgumentNullException(nameof(httpFactory));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GenAiUpstreamResponse?> RecommendAsync(
        GenAiUpstreamRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var baseUrl = _configuration["Services:GenAiBaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl)
            || baseUrl.Contains("REEMPLAZAR", StringComparison.OrdinalIgnoreCase)
            || !Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
        {
            return null;
        }

        try
        {
            var client = _httpFactory.CreateClient("GenAiService");
            var targetUri = new Uri(baseUri, "recommendations");
            using var msg = new HttpRequestMessage(HttpMethod.Post, targetUri);
            var internalKey = _configuration["InternalService:Key"];
            if (!string.IsNullOrWhiteSpace(internalKey)
                && !internalKey.Contains("REEMPLAZAR", StringComparison.OrdinalIgnoreCase))
            {
                msg.Headers.Add("X-Internal-Key", internalKey);
            }

            msg.Content = JsonContent.Create(request);
            using var resp = await client.SendAsync(msg, ct).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                LogGenAiFallback(_logger, $"HTTP {(int)resp.StatusCode}", null);
                return null;
            }

            return await resp.Content.ReadFromJsonAsync<GenAiUpstreamResponse>(cancellationToken: ct)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            LogGenAiFallback(_logger, ex.Message, ex);
            return null;
        }
        catch (TaskCanceledException ex)
        {
            LogGenAiFallback(_logger, ex.Message, ex);
            return null;
        }
        catch (JsonException ex)
        {
            LogGenAiFallback(_logger, ex.Message, ex);
            return null;
        }
    }
}
