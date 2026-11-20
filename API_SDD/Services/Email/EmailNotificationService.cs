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
    Task SendEmailVerificationAsync(string recipientEmail, string recipientName, string rawToken, CancellationToken cancellationToken);
}

/// <summary>
/// Envío SMTP cifrado cuando existe configuración externa; sin fugas de correo o token en logs.
/// </summary>
[SuppressMessage(
    "Design",
    "CA1031:Do not catch general exception types",
    Justification = "Recuperación de contraseña mantiene respuesta neutra anti-enumeración aunque falle el transporte SMTP.")]
[SuppressMessage(
    "Minor Code Smell",
    "S1075:URIs should not be hardcoded",
    Justification = "Ruta de fallback por defecto cuando no se define VerificationBaseUrl en configuración externa.")]
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

    public async Task SendEmailVerificationAsync(
        string recipientEmail,
        string recipientName,
        string rawToken,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(rawToken);

        var smtpHost = _configuration["Email:SmtpHost"];
        if (string.IsNullOrWhiteSpace(smtpHost)
            || smtpHost.Contains("REEMPLAZAR", StringComparison.OrdinalIgnoreCase))
        {
            LogSmtpVerificationSkipped(_logger);
            return;
        }

        var portRaw = _configuration["Email:SmtpPort"];
        var smtpPort = int.TryParse(portRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 587;
        var smtpUser = _configuration["Email:SmtpUser"];
        var smtpPass = _configuration["Email:SmtpPassword"];
        var fromEmail = _configuration["Email:FromAddress"] ?? "no-reply@bustrack.app";
        var baseUrl = _configuration["Email:VerificationBaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            baseUrl = "https://bustrack.app/verificar-correo";
        }
#pragma warning disable S1075 // URI de fallback configurable
        var verificationUrl = $"{baseUrl.TrimEnd('/')}?token={Uri.EscapeDataString(rawToken)}";
#pragma warning restore S1075
        var displayName = string.IsNullOrWhiteSpace(recipientName) ? "Usuario" : recipientName.Trim();

        try
        {
            var bodyHtml = $@"<!DOCTYPE html>
<html lang=""es"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Verifica tu correo en BusTrack</title>
  <style>
    body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f4f6f8; margin: 0; padding: 20px; color: #1e293b; }}
    .container {{ max-width: 580px; margin: 0 auto; background: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1); }}
    .header {{ background: #0f172a; padding: 28px 24px; text-align: center; }}
    .logo {{ color: #38bdf8; font-size: 24px; font-weight: 800; letter-spacing: 0.5px; text-decoration: none; }}
    .content {{ padding: 32px 28px; line-height: 1.6; }}
    .greeting {{ font-size: 18px; font-weight: 600; margin-bottom: 16px; color: #0f172a; }}
    .button-container {{ text-align: center; margin: 32px 0; }}
    .button {{ background: #0284c7; color: #ffffff !important; padding: 14px 28px; border-radius: 8px; font-weight: 600; text-decoration: none; display: inline-block; font-size: 16px; }}
    .link-alt {{ word-break: break-all; font-size: 13px; color: #0284c7; }}
    .notice {{ background: #f8fafc; border-left: 4px solid #0284c7; padding: 14px; font-size: 13px; color: #64748b; margin: 24px 0; border-radius: 4px; }}
    .footer {{ background: #f1f5f9; padding: 20px; text-align: center; font-size: 12px; color: #94a3b8; }}
  </style>
</head>
<body>
  <div class=""container"">
    <div class=""header"">
      <span class=""logo"">BusTrack</span>
    </div>
    <div class=""content"">
      <div class=""greeting"">¡Hola, {System.Net.WebUtility.HtmlEncode(displayName)}!</div>
      <p>Gracias por unirte a BusTrack, la red confiable de información de transporte y rutas.</p>
      <p>Para activar tu cuenta y empezar a planear tus viajes de manera segura, confirma que este es tu correo electrónico:</p>
      <div class=""button-container"">
        <a href=""{verificationUrl}"" class=""button"" target=""_blank"" rel=""noopener noreferrer"">Verificar mi correo electrónico</a>
      </div>
      <p style=""font-size: 14px; color: #475569;"">Si el botón no funciona en tu dispositivo, copia y pega el siguiente enlace en tu navegador:</p>
      <p><a href=""{verificationUrl}"" class=""link-alt"">{verificationUrl}</a></p>
      <div class=""notice"">
        <strong>Vigencia:</strong> Este enlace expirará en 24 horas por motivos de seguridad.<br>
        <strong>Aviso:</strong> Si no creaste una cuenta en BusTrack, puedes desestimar este mensaje de forma segura.
      </div>
    </div>
    <div class=""footer"">
      &copy; {DateTime.UtcNow.Year} BusTrack API · Todos los derechos reservados.
    </div>
  </div>
</body>
</html>";

            using var message = new MailMessage(fromEmail, recipientEmail)
            {
                Subject = "BusTrack — Confirma tu correo electrónico",
                Body = bodyHtml,
                IsBodyHtml = true,
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
            LogSmtpVerificationSent(_logger);
        }
        catch (Exception ex)
        {
            LogSmtpVerificationFailed(_logger, ex);
        }
    }

    [LoggerMessage(EventId = 2004, Level = LogLevel.Information, Message = "Notificación de verificación de correo SMTP omitida por ambiente sin SMTP externo.")]
    private static partial void LogSmtpVerificationSkipped(ILogger logger);

    [LoggerMessage(EventId = 2005, Level = LogLevel.Information, Message = "Correo de verificación SMTP entregado correctamente.")]
    private static partial void LogSmtpVerificationSent(ILogger logger);

    [LoggerMessage(EventId = 2006, Level = LogLevel.Warning, Message = "No fue posible entregar correo SMTP de verificación.")]
    private static partial void LogSmtpVerificationFailed(ILogger logger, Exception ex);

    [LoggerMessage(EventId = 2001, Level = LogLevel.Information, Message = "Notificación de recuperación SMTP omitida por ambiente sin SMTP externo.")]
    private static partial void LogSmtpSkipped(ILogger logger);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Information, Message = "Notificación de recuperación SMTP entregada correctamente.")]
    private static partial void LogSmtpSent(ILogger logger);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Warning, Message = "No fue posible entregar correo SMTP de recuperación.")]
    private static partial void LogSmtpFailed(ILogger logger, Exception ex);
}
