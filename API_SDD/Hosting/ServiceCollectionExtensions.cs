using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using ApiSDD.Common;
using ApiSDD.Data;
using ApiSDD.Health;
using ApiSDD.OpenApi;
using ApiSDD.Security;
using ApiSDD.Serialization;
using ApiSDD.Services;
using ApiSDD.Services.Email;
using ApiSDD.Services.Integrations;
using ApiSDD.Services.Moderation;
using ApiSDD.Services.NoSql;
using ApiSDD.Services.Notifications;
using ApiSDD.Services.Storage;
using ApiSDD.Validators;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
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

        services.AddControllers().AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.PropertyNamingPolicy = null;
            o.JsonSerializerOptions.TypeInfoResolverChain.Insert(0, BusTrackJsonContext.Default);
            o.JsonSerializerOptions.TypeInfoResolverChain.Add(new DefaultJsonTypeInfoResolver());
        });

        services.AddResponseCompression(o =>
        {
            o.EnableForHttps = true;
            o.Providers.Add<BrotliCompressionProvider>();
            o.Providers.Add<GzipCompressionProvider>();
        });
        services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
        services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
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

        var isMongoPrimary = string.Equals(
            configuration["Persistence:Provider"],
            "MongoDB",
            StringComparison.OrdinalIgnoreCase);

        ConfigurePersistenceAndServices(services, configuration, environment, isMongoPrimary);

        return services;
    }

    private static void ConfigurePersistenceAndServices(
            IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment,
            bool isMongoPrimary)
        {
            var connectionString = configuration.GetConnectionString("Default");
            if (!isMongoPrimary && string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:Default is required (external SQL).");
            }

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                services.AddDbContext<AppDbContext>(o => o.UseSqlServer(
                    connectionString,
                    sql =>
                    {
                        sql.EnableRetryOnFailure();
                        sql.CommandTimeout(60);
                    }));
            }

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

            var healthChecks = services.AddHealthChecks()
                .AddCheck<MongoDbHealthCheck>("mongodb")
                .AddCheck<RedisHealthCheck>("redis")
                .AddCheck<AssemblyIntegrityCheck>("assembly");
            if (!isMongoPrimary && !string.IsNullOrWhiteSpace(connectionString))
            {
                healthChecks.AddCheck<SqlServerHealthCheck>("sql");
            }

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
            services.AddAuthorization(o => o.AddPolicy(
                "AdminPolicy",
                p => p.RequireAuthenticatedUser().RequireRole("Admin")));
            services.AddSingleton(TimeProvider.System);
            services.AddSingleton<IPasswordHasher, Argon2idPasswordHasher>();
            services.AddScoped<ISDDUsuarioService, SDDUsuarioService>();

            // NV-05 / NV-06: JWT + Blacklist distribuida Redis (Req. 10 & 15)
            services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
            services.AddSingleton<IJwtTokenService, JwtTokenService>();
            services.AddSingleton<ITokenRevocationService>(sp => new TokenRevocationService(
                sp.GetRequiredService<IDistributedCache>(),
                sp.GetRequiredService<TimeProvider>()));
            services.AddScoped<IEmailNotificationService, EmailNotificationService>();

            // Repositorio NoSQL unificado (MongoDB Atlas en nube; almacén en memoria en pruebas aisladas).
            services.AddSingleton<ApiSDD.Data.MongoDbContext>();
            services.AddHostedService<ApiSDD.Data.MongoIndexInitializer>();

            var redisCs = ConfigurationSanitizer.CleanSecret(configuration["Redis:ConnectionString"]);
            if (ConfigurationSanitizer.HasUsableValue(redisCs))
            {
                services.AddStackExchangeRedisCache(o =>
                {
                    o.Configuration = redisCs;
                    o.InstanceName = "bustrack:";
                });
            }
            else
            {
                services.AddOptions();
                services.AddSingleton<MemoryDistributedCache>();
                services.AddSingleton<IDistributedCache>(sp =>
                {
                    var mongo = sp.GetRequiredService<ApiSDD.Data.MongoDbContext>();
                    return mongo.IsConfigured
                        ? new ApiSDD.Data.NoSql.MongoDistributedCache(mongo, sp.GetRequiredService<TimeProvider>())
                        : sp.GetRequiredService<MemoryDistributedCache>();
                });
            }

            services.AddSingleton<ApiSDD.Data.NoSql.InMemoryBusTrackStore>();
            services.AddSingleton<ApiSDD.Data.NoSql.MongoBusTrackStore>();
            services.AddSingleton<ApiSDD.Data.NoSql.IBusTrackNoSqlStore>(sp =>
            {
                var mongo = sp.GetRequiredService<ApiSDD.Data.MongoDbContext>();
                return mongo.IsConfigured
                    ? sp.GetRequiredService<ApiSDD.Data.NoSql.MongoBusTrackStore>()
                    : sp.GetRequiredService<ApiSDD.Data.NoSql.InMemoryBusTrackStore>();
            });

            services.AddScoped<ApiSDD.Services.Empresa.IEmpresaService, ApiSDD.Services.Empresa.EmpresaService>();
            services.AddScoped<ITransportCatalogService, TransportCatalogService>();
            services.AddScoped<IContentModerationService, ContentModerationService>();
            services.AddScoped<IBlobMediaService, BlobMediaService>();
            services.AddHttpClient("Fcm").AddStandardResilienceHandler();
            services.AddHttpClient("MlService").AddStandardResilienceHandler();
            services.AddHttpClient("GenAiService").AddStandardResilienceHandler();
            services.AddScoped<IMlServiceClient, MlServiceClient>();
            services.AddScoped<IGenAiServiceClient, GenAiServiceClient>();
            services.AddScoped<IPushNotificationService, PushNotificationService>();
            services.AddScoped<IUserInteractionService, UserInteractionService>();
            services.AddScoped<IAdminCatalogService, AdminCatalogService>();
            services.AddScoped<IIntelligenceService, IntelligenceService>();

            if (isMongoPrimary)
            {
                services.AddScoped<IAuthService, MongoAuthService>();
                services.AddScoped<IUsuarioPerfilService, MongoUsuarioPerfilService>();
                services.AddScoped<ApiSDD.Services.Empresa.IEmpresaSqlService, MongoEmpresaService>();
                services.AddScoped<ApiSDD.Services.MedioTransporte.IMedioTransporteService, MongoMedioTransporteService>();
            }
            else
            {
                services.AddScoped<IAuthService, AuthService>();
                services.AddScoped<IUsuarioPerfilService, UsuarioPerfilService>();
                services.AddScoped<ApiSDD.Services.Empresa.IEmpresaSqlService, ApiSDD.Services.Empresa.EmpresaSqlService>();
                services.AddScoped<ApiSDD.Services.MedioTransporte.IMedioTransporteService, ApiSDD.Services.MedioTransporte.MedioTransporteService>();
            }
        }

    private static void AddJwt(IServiceCollection services, IConfiguration configuration)
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

    private static void FixedWindow(FixedWindowRateLimiterOptions options, int permits, int minutes)
    {
        options.PermitLimit = permits;
        options.Window = TimeSpan.FromMinutes(minutes);
        options.QueueLimit = 0;
        options.AutoReplenishment = true;
    }
}
