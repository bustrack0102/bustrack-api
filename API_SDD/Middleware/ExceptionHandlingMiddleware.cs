using System.Diagnostics;
using ApiSDD.Common;
using FluentValidation;

namespace ApiSDD.Middleware;

/// <summary>
/// Único manejador de errores: mapea a códigos uniformes, Detail solo no-prod,
/// relanza si la respuesta ya empezó. Cero try/catch en controllers.
/// </summary>
internal sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        IWebHostEnvironment environment,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            await WriteErrorAsync(context, ex).ConfigureAwait(false);
        }
    }

    internal async Task WriteErrorAsync(HttpContext context, Exception ex)
    {
        var (status, error) = Map(ex);
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        LogMapped(_logger, traceId, status, ex);

        var detail = _environment.IsProduction() ? null : ex.Message;
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(
            new ErrorResponse(error, status, traceId, detail),
            ApiJson.Options,
            context.RequestAborted).ConfigureAwait(false);
    }

    internal static (int Status, string Error) Map(Exception ex) => ex switch
    {
        HttpErrorException mapped => (mapped.StatusCode, mapped.Error),
        ValidationException => (StatusCodes.Status422UnprocessableEntity, "validation"),
        BadHttpRequestException => (StatusCodes.Status400BadRequest, "bad_request"),
        OperationCanceledException => (StatusCodes.Status408RequestTimeout, "timeout"),
        _ => (StatusCodes.Status500InternalServerError, "internal"),
    };

    private static readonly Action<ILogger, string, int, Exception?> LogMapped =
        LoggerMessage.Define<string, int>(
            LogLevel.Error,
            new EventId(1, nameof(WriteErrorAsync)),
            "Unhandled exception {TraceId} mapped to {Status}");
}
