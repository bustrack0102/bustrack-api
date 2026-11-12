using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Mail;

namespace ApiSDD.Services.Email;

/// <summary>
/// Contrato de notificación de recuperación de contraseña (BE-35 / EP-AU-05).
/// </summary>
public interface IEmailNotificationService
{
    Task SendPasswordResetAsync(string recipientEmail, string resetCodeOrToken, CancellationToken cancellationToken);
}

/// <summary>
/// Envío SMTP cifrado cuando existe configuración externa; sin fugas de correo o token en logs.
/// </summary>
[SuppressMessage(
    "Design",
    "CA1031:Do not catch general exception types",
    Justification = "Recuperación de contraseña mantiene respuesta neutra anti-enumeración aunque falle el transporte SMTP.")]
internal sealed partial class EmailNotificationService : IEmailNotificationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(IConfiguration configuration, ILogger<EmailNotificationService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SendPasswordResetAsync(
        string recipientEmail,
        string resetCodeOrToken,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(resetCodeOrToken);

        var smtpHost = _configuration["Email:SmtpHost"];
        if (string.IsNullOrWhiteSpace(smtpHost)
            || smtpHost.Contains("REEMPLAZAR", StringComparison.OrdinalIgnoreCase))
        {
            LogSmtpSkipped(_logger);
            return;
        }

        var portRaw = _configuration["Email:SmtpPort"];
        var smtpPort = int.TryParse(portRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 587;
        var smtpUser = _configuration["Email:SmtpUser"];
        var smtpPass = _configuration["Email:SmtpPassword"];
        var fromEmail = _configuration["Email:FromAddress"] ?? "no-reply@bustrack.app";

        try
        {
            using var message = new MailMessage(fromEmail, recipientEmail)
            {
                Subject = "BusTrack — Recuperación de contraseña",
                Body = $"Usa este código temporal (vigente 30 minutos) para restablecer tu acceso en BusTrack: {resetCodeOrToken}",
                IsBodyHtml = false,
            };

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 10000,
            };
            if (!string.IsNullOrWhiteSpace(smtpUser) && !string.IsNullOrWhiteSpace(smtpPass))
            {
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);
            }

            await client.SendMailAsync(message, cancellationToken).ConfigureAwait(false);
            LogSmtpSent(_logger);
        }
        catch (Exception ex)
        {
            LogSmtpFailed(_logger, ex);
        }
    }

    [LoggerMessage(EventId = 2001, Level = LogLevel.Information, Message = "Notificación de recuperación SMTP omitida por ambiente sin SMTP externo.")]
    private static partial void LogSmtpSkipped(ILogger logger);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Information, Message = "Notificación de recuperación SMTP entregada correctamente.")]
    private static partial void LogSmtpSent(ILogger logger);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Warning, Message = "No fue posible entregar correo SMTP de recuperación.")]
    private static partial void LogSmtpFailed(ILogger logger, Exception ex);
}
