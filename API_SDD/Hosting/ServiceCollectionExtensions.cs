using System.Diagnostics;
using System.Text;
using ApiSDD.Common;
using ApiSDD.Data;
using ApiSDD.Health;
using ApiSDD.OpenApi;
using ApiSDD.Security;
using ApiSDD.Services;
using ApiSDD.Validators;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;

namespace ApiSDD.Hosting;

/// <summary>Registro testeable de servicios (NV-01 R1 + R4). Fail-fast ante config ausente.</summary>
internal static class ServiceCollectionExtensions
{
    public const string CorsPolicy = "SingleOrigin";

    public static IServiceCollection AddWebApiServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = null);
        services.AddOpenApi(o => o
            .AddDocumentTransformer<ApiVersionDocumentTransformer>()
            .AddDocumentTransformer<SecuritySchemeDocumentTransformer>());
        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        services.AddValidatorsFromAssembly(AssemblyMarker.Value);

        services.AddApiVersioning(o =>
        {
            o.DefaultApiVersion = new ApiVersion(1, 0);
            o.AssumeDefaultVersionWhenUnspecified = false;
            o.ReportApiVersions = true;
            o.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddMvc();

        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(o => o.AddPolicy(CorsPolicy, p => p
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()));

        var rate = configuration.GetSection("RateLimiting");
        var global = rate.GetValue<int?>("GlobalPerMinute") ?? 1000;
        var login = rate.GetValue<int?>("LoginPer5Minutes") ?? 5;
        var login2fa = rate.GetValue<int?>("Login2faPer5Minutes") ?? 10;
        var admin = rate.GetValue<int?>("AdminPerMinute") ?? 200;
        var concurrent = rate.GetValue<int?>("ConcurrentWrites") ?? 10;
        services.AddRateLimiter(o =>
        {
            o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            o.OnRejected = static (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.Headers.RetryAfter = "60";
                var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
                return new ValueTask(context.HttpContext.Response.WriteAsJsonAsync(
                    new ErrorResponse("rate_limited", 429, traceId, null),
                    ApiJson.Options,
                    token));
            };
            o.AddFixedWindowLimiter("Global", options => FixedWindow(options, global, 1));
            o.AddFixedWindowLimiter("Login", options => FixedWindow(options, login, 5));
            o.AddFixedWindowLimiter("Login2faVerify", options => FixedWindow(options, login2fa, 5));
            o.AddFixedWindowLimiter("Admin", options => FixedWindow(options, admin, 1));
            o.AddConcurrencyLimiter("ConcurrentWrites", options =>
            {
                options.PermitLimit = concurrent;
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 0;
            });
        });

        if (configuration.GetValue("UseJwtBearer", true))
        {
            AddJwt(services, configuration);
        }

        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:Default is required (external SQL).");
        }

        services.AddDbContext<AppDbContext>(o => o.UseSqlServer(
            connectionString,
            sql =>
            {
                sql.EnableRetryOnFailure();
                sql.CommandTimeout(60);
            }));

        var keysPath = configuration["DataProtection:KeysPath"];
        var dataProtection = services.AddDataProtection();
        if (!string.IsNullOrWhiteSpace(keysPath))
        {
            Directory.CreateDirectory(keysPath);
            dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keysPath));
        }
        else if (environment.IsProduction())
        {
            throw new InvalidOperationException("DataProtection:KeysPath is required in Production.");
        }

        services.AddHealthChecks()
            .AddCheck<SqlServerHealthCheck>("sql")
            .AddCheck<AssemblyIntegrityCheck>("assembly");

        services.AddRequestTimeouts(o => o.DefaultPolicy = new RequestTimeoutPolicy
        {
            Timeout = TimeSpan.FromSeconds(60),
            TimeoutStatusCode = StatusCodes.Status408RequestTimeout,
        });

        services.AddHsts(o =>
        {
            o.MaxAge = TimeSpan.FromDays(365);
            o.IncludeSubDomains = true;
            o.Preload = true;
        });

        // NV-04: slice wiring without touching the pipeline order above.
        // AdminPolicy is owned by nv-05 (login); nv-04 ships the minimal role gate.
        services.AddAuthorization(o => o.AddPolicy(
            "AdminPolicy",
            p => p.RequireAuthenticatedUser().RequireRole("Admin")));
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHasher, Argon2idPasswordHasher>();
        services.AddScoped<ISDDUsuarioService, SDDUsuarioService>();

        // NV-05: login wiring without touching the pipeline order above.
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        // NV-06: logout revocation without touching the pipeline order above.
        services.AddSingleton<ITokenRevocationService, TokenRevocationService>();

        return services;

        static void AddJwt(IServiceCollection services, IConfiguration configuration)
        {
            var key = configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key is required when UseJwtBearer is enabled.");
            if (Encoding.UTF8.GetByteCount(key) < 32)
            {
                throw new InvalidOperationException("Jwt:Key must decode to at least 32 bytes.");
            }

            var issuer = configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("Jwt:Issuer is required when UseJwtBearer is enabled.");
            var audience = configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("Jwt:Audience is required when UseJwtBearer is enabled.");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    // Keep wire claim names (sub/jti/role): no WS-Federation remapping.
                    o.MapInboundClaims = false;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudience = audience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                        ValidAlgorithms = ["HS256"],
                        ClockSkew = TimeSpan.Zero,
                        NameClaimType = "sub",
                        RoleClaimType = "role",
                    };
                    o.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            var jti = context.Principal?.FindFirst("jti")?.Value;
                            if (jti is not null)
                            {
                                var revocation = context.HttpContext.RequestServices
                                    .GetRequiredService<ITokenRevocationService>();
                                if (await revocation.IsRevokedAsync(jti, context.HttpContext.RequestAborted)
                                        .ConfigureAwait(false))
                                {
                                    context.Fail("Token revoked.");
                                }
                            }
                        },
                    };
                });
        }

        static void FixedWindow(FixedWindowRateLimiterOptions options, int permits, int minutes)
        {
            options.PermitLimit = permits;
            options.Window = TimeSpan.FromMinutes(minutes);
            options.QueueLimit = 0;
            options.AutoReplenishment = true;
        }
    }
}
