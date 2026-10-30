using System.Reflection;
using System.Text.Json;
using ApiSDD.Common;
using ApiSDD.Dtos;
using ApiSDD.Hosting;
using ApiSDD.Validators;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace UnitTest.Architecture;

/// <summary>NV-02: molde DTO→Validator→Service sin endpoints (5).</summary>
public sealed class VerticalSliceConventionTests : IDisposable
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

    private static Assembly ApiAssembly => typeof(PagedResult<int>).Assembly;

    private ServiceProvider BuildFullProvider()
    {
        var values = new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "Server=x;Database=x;TrustServerCertificate=True;",
            ["Jwt:Key"] = Common.ApiFactory.TestJwtKey,
            ["Jwt:Issuer"] = "test-issuer",
            ["Jwt:Audience"] = "test-audience",
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var services = new ServiceCollection();
        services.AddWebApiServices(config, new TestEnvironment());
        var provider = services.BuildServiceProvider();
        _disposables.Add(provider);
        return provider;
    }

    [Fact]
    public void PagedResultExponeContratoUniforme()
    {
        var page = new PagedResult<int>([1, 2], 2, 1, 20);

        page.Items.Should().Equal(1, 2);
        page.TotalCount.Should().Be(2);
        page.Page.Should().Be(1);
        page.PageSize.Should().Be(20);

        var json = JsonSerializer.Serialize(page, ApiJson.Options);
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("Items").GetArrayLength().Should().Be(2);
        doc.RootElement.GetProperty("TotalCount").GetInt32().Should().Be(2);
        doc.RootElement.GetProperty("Page").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("PageSize").GetInt32().Should().Be(20);
    }

    [Fact]
    public void ServicesPublicosSinCacheEntidades()
    {
        typeof(ApiSDD.Services.IService).Should().NotBeNull("la capa service queda marcada");

        var services = ApiAssembly.GetTypes()
            .Where(static t => t.Namespace == "ApiSDD.Services" && t.IsClass && !t.IsAbstract)
            .ToList();

        foreach (var service in services)
        {
            UsesMemoryCache(service).Should().BeFalse("ningún service usa IMemoryCache de entidades");
        }
    }

    [Fact]
    public void ValidatorsEspejoDeDto()
    {
        var validators = ApiAssembly.GetTypes()
            .Where(static t => t.Namespace == "ApiSDD.Validators" && t.IsClass && !t.IsAbstract && !t.IsNested)
            .ToList();

        validators.Should().NotBeEmpty("PaginationQueryValidator demuestra el molde");
        foreach (var validator in validators)
        {
            InheritsAbstractValidator(validator).Should().BeTrue("cada validator es espejo AbstractValidator<TDto>");
        }

        var services = new ServiceCollection();
        services.AddValidatorsFromAssembly(ApiAssembly);
        using var provider = services.BuildServiceProvider();
        var resolved = provider.GetRequiredService<IValidator<PaginationQuery>>();

        resolved.Should().BeOfType<PaginationQueryValidator>();
        resolved.Validate(new PaginationQuery(1, 20)).IsValid.Should().BeTrue();
        resolved.Validate(new PaginationQuery(0, 500)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void ControllersSinTryCatchAdHoc()
    {
        var files = ControllerSources();

        files.Should().HaveCountGreaterThanOrEqualTo(2, "Ping + Probe existen");
        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            text.Should().NotMatchRegex(@"\btry\b", $"{file} delega errores al middleware");
            text.Should().NotMatchRegex(@"\bcatch\b", $"{file} delega errores al middleware");
        }
    }

    [Fact]
    public void DICableaValidatorsDelAssembly()
    {
        var provider = BuildFullProvider();

        var validator = provider.GetService<IValidator<PaginationQuery>>();

        validator.Should().NotBeNull("AddWebApiServices registra los validators espejo");
    }

    public void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
    }

    private static bool UsesMemoryCache(Type service)
    {
        var cache = typeof(IMemoryCache);
        foreach (var constructor in service.GetConstructors())
        {
            foreach (var parameter in constructor.GetParameters())
            {
                if (parameter.ParameterType == cache)
                {
                    return true;
                }
            }
        }

        const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        foreach (var field in service.GetFields(Instance))
        {
            if (field.FieldType == cache)
            {
                return true;
            }
        }

        foreach (var property in service.GetProperties(Instance))
        {
            if (property.PropertyType == cache)
            {
                return true;
            }
        }

        return false;
    }

    private static bool InheritsAbstractValidator(Type type)
    {
        var current = type.BaseType;
        while (current is not null)
        {
            if (current.IsGenericType
                && current.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    private static string[] ControllerSources()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        for (var depth = 0; depth < 8 && directory is not null; depth++)
        {
            var candidate = Path.Combine(directory.FullName, "API_SDD", "Controllers");
            if (Directory.Exists(candidate))
            {
                return Directory.GetFiles(candidate, "*Controller.cs", SearchOption.AllDirectories);
            }

            directory = directory.Parent;
        }

        return [];
    }
}
