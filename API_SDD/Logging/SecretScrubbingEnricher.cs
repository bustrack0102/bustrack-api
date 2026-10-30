using Serilog.Core;
using Serilog.Events;

namespace ApiSDD.Logging;

/// <summary>
/// Redacta valores de propiedades cuyos nombres huelen a secreto.
/// Primera línea de defensa en código; critic B1 + AppSettingsTests la respaldan.
/// </summary>
internal sealed class SecretScrubbingEnricher : ILogEventEnricher
{
    private static readonly string[] Sensitive =
    [
        "password", "passwd", "pwd", "secret", "token", "apikey", "connectionstring",
    ];

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        ArgumentNullException.ThrowIfNull(propertyFactory);

        foreach (var name in logEvent.Properties.Keys
                     .Where(k => Sensitive.Any(s => k.Contains(s, StringComparison.OrdinalIgnoreCase)))
                     .ToList())
        {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(name, "***"));
        }
    }
}
