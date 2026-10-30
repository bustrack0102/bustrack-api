using ApiSDD.Health;
using ApiSDD.Hosting;
using ApiSDD.Logging;
using ApiSDD.Middleware;
using Asp.Versioning;
using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Local-only overrides (git-ignored): opt-in via UseLocalSettings so test
// hosts never pick up machine secrets (WAF must keep its own settings).
if (builder.Configuration.GetValue("UseLocalSettings", false))
{
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
}

// NV-01 R4: Kestrel 10MB / 2min (configurable, spec defaults).
builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.Limits.MaxRequestBodySize =
        context.Configuration.GetValue<long?>("Kestrel:Limits:MaxRequestBodySize") ?? 10L * 1024 * 1024;
    options.Limits.KeepAliveTimeout =
        context.Configuration.GetValue<TimeSpan?>("Kestrel:Limits:KeepAliveTimeout") ?? TimeSpan.FromMinutes(2);
});

if (int.TryParse(builder.Configuration["PORT"], out var listenPort))
{
    builder.WebHost.UseUrls($"http://*:{listenPort}");
}

// NV-01: Serilog JSON + consola + filtro secretos + CorrelationId.
builder.Host.UseSerilog((context, _, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .Enrich.With<SecretScrubbingEnricher>()
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
    .WriteTo.File(
        new Serilog.Formatting.Json.JsonFormatter(),
        "logs/api-.json",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7));

builder.Services.AddWebApiServices(builder.Configuration, builder.Environment);

var app = builder.Build();

// NV-01 R1: pipeline fijo. CorrelationId/Serilog/Timeouts son adiciones
// documentadas (Memoria.md), no reordenamientos.
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseRequestTimeouts();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

if (app.Environment.IsProduction())
{
    // Testing/Staging omiten redirección (TestServer es HTTP; ver Memoria.md).
    app.UseHttpsRedirection();
}

app.UseCors(ServiceCollectionExtensions.CorsPolicy);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapHealthChecks("/health", new() { Predicate = static _ => false });
app.MapHealthChecks("/health/ready", new()
{
    Predicate = static _ => true,
    ResponseWriter = HealthResponses.WriteReadyAsync,
});

await app.RunAsync().ConfigureAwait(false);

public partial class Program
{
    protected Program()
    {
    }
}
