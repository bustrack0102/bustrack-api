using ApiSDD.Hosting;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace UnitTest.DI;

/// <summary>NV-01 Unit/DI 3: CORS registrado, sin AllowAnyOrigin, orígenes configurables.</summary>
public sealed class ServiceRegistrationTests : IDisposable
{
    private readonly List<IDisposable> _disposables = [];

    private sealed class TestEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "API_SDD";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } =
            new PhysicalFileProvider(AppContext.BaseDirectory);
    }

    private ServiceProvider Build(Dictionary<string, string?> values)
    {
        values["ConnectionStrings:Default"] = "Server=x;Database=x;TrustServerCertificate=True;";
        values["Jwt:Key"] = UnitTest.Common.ApiFactory.TestJwtKey;
        values["Jwt:Issuer"] = "test-issuer";
        values["Jwt:Audience"] = "test-audience";
        var config = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var services = new ServiceCollection();
        services.AddWebApiServices(config, new TestEnvironment());
        var provider = services.BuildServiceProvider();
        _disposables.Add(provider);
        return provider;
    }

    [Fact]
    public void CorsRegistradoConPoliticaUnica()
    {
        var provider = Build(new Dictionary<string, string?>());
        var options = provider.GetRequiredService<IOptions<CorsOptions>>().Value;
        var policy = options.GetPolicy(ServiceCollectionExtensions.CorsPolicy);
        policy.Should().NotBeNull();
    }

    [Fact]
    public void SinAllowAnyOriginPorDefecto()
    {
        var provider = Build(new Dictionary<string, string?>());
        var policy = provider.GetRequiredService<IOptions<CorsOptions>>().Value
            .GetPolicy(ServiceCollectionExtensions.CorsPolicy);
        policy.Should().NotBeNull();
        policy.IsOriginAllowed("https://evil.test").Should().BeFalse();
    }

    [Fact]
    public void OrigenesConfigurablesPorEnv()
    {
        var provider = Build(new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = "https://app.test",
        });
        var policy = provider.GetRequiredService<IOptions<CorsOptions>>().Value
            .GetPolicy(ServiceCollectionExtensions.CorsPolicy);
        policy.Should().NotBeNull();
        policy.Origins.Should().Contain("https://app.test");
        policy.IsOriginAllowed("https://app.test").Should().BeTrue();
    }

    public void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
    }
}
