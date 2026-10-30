namespace ApiSDD.Middleware;

/// <summary>Propaga X-Correlation-Id desde el TraceIdentifier.</summary>
internal sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", context.TraceIdentifier))
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[HeaderName] = context.TraceIdentifier;
                return Task.CompletedTask;
            });
            await _next(context).ConfigureAwait(false);
        }
    }
}
