using System.Text.Json;
using ApiSDD.Common;
using ApiSDD.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace UnitTest.Middleware;

/// <summary>NV-01 Unit/Errors 14: tabla 8 + nulos + passthrough + Detail prod/dev + HasStarted.</summary>
public sealed class ErrorMappingTests
{
    private static ExceptionHandlingMiddleware Arrange(
        RequestDelegate next,
        string environment = "Development") =>
        new(next, MockEnv(environment), NullLogger<ExceptionHandlingMiddleware>.Instance);

    private static IWebHostEnvironment MockEnv(string name)
    {
        var mock = new Mock<IWebHostEnvironment>();
        mock.SetupGet(e => e.EnvironmentName).Returns(name);
        return mock.Object;
    }

    [Theory]
    [InlineData(typeof(NotFoundException), 404, "not_found")]
    [InlineData(typeof(UnauthorizedException), 401, "invalid_credentials")]
    [InlineData(typeof(ForbiddenException), 403, "forbidden")]
    [InlineData(typeof(ConflictException), 409, "conflict")]
    [InlineData(typeof(RequestValidationException), 422, "validation")]
    [InlineData(typeof(FluentValidation.ValidationException), 422, "validation")]
    [InlineData(typeof(BadHttpRequestException), 400, "bad_request")]
    [InlineData(typeof(OperationCanceledException), 408, "timeout")]
    [InlineData(typeof(InvalidOperationException), 500, "internal")]
    public async Task TablaMapeaCodigoYError(Type failureType, int status, string error)
    {
        var failure = (Exception)Activator.CreateInstance(failureType, "boom")!;
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = Arrange(_ => Task.FromException(failure));
        await middleware.InvokeAsync(context).ConfigureAwait(true);

        context.Response.StatusCode.Should().Be(status);
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var doc = await JsonDocument.ParseAsync(context.Response.Body).ConfigureAwait(true);
        doc.RootElement.GetProperty("error").GetString().Should().Be(error);
        doc.RootElement.GetProperty("status").GetInt32().Should().Be(status);
    }

    [Fact]
    public async Task DetailNuloSeOmiteDelJson()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = Arrange(_ => Task.FromException(new NotFoundException(null!)), "Production");
        await middleware.InvokeAsync(context).ConfigureAwait(true);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var doc = await JsonDocument.ParseAsync(context.Response.Body).ConfigureAwait(true);
        doc.RootElement.TryGetProperty("detail", out _).Should().BeFalse();
    }

    [Fact]
    public async Task TraceIdCaeATraceIdentifierSinActivity()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = Arrange(_ => Task.FromException(new InvalidOperationException("x")));
        await middleware.InvokeAsync(context).ConfigureAwait(true);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var doc = await JsonDocument.ParseAsync(context.Response.Body).ConfigureAwait(true);
        doc.RootElement.GetProperty("traceId").GetString().Should().Be(context.TraceIdentifier);
    }

    [Fact]
    public async Task PassthroughSinExcepcion()
    {
        var invoked = false;
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = Arrange(ctx =>
        {
            invoked = true;
            ctx.Response.StatusCode = 200;
            return Task.CompletedTask;
        });
        await middleware.InvokeAsync(context).ConfigureAwait(true);

        invoked.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task DetailPresenteEnDevelopment()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = Arrange(
            _ => Task.FromException(new InvalidOperationException("sensitive-detail")),
            "Development");
        await middleware.InvokeAsync(context).ConfigureAwait(true);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var doc = await JsonDocument.ParseAsync(context.Response.Body).ConfigureAwait(true);
        doc.RootElement.GetProperty("detail").GetString().Should().Be("sensitive-detail");
    }

    [Fact]
    public async Task DetailAusenteEnProduction()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = Arrange(
            _ => Task.FromException(new InvalidOperationException("sensitive-detail")),
            "Production");
        await middleware.InvokeAsync(context).ConfigureAwait(true);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var doc = await JsonDocument.ParseAsync(context.Response.Body).ConfigureAwait(true);
        doc.RootElement.TryGetProperty("detail", out _).Should().BeFalse();
    }

    [Fact]
    public async Task RespuestaIniciadaRelanza()
    {
        var response = new Mock<HttpResponse>();
        response.SetupGet(r => r.HasStarted).Returns(true);
        var context = new Mock<HttpContext>();
        context.SetupGet(c => c.Response).Returns(response.Object);
        context.SetupGet(c => c.RequestAborted).Returns(CancellationToken.None);
        var middleware = Arrange(
            _ => Task.FromException(new InvalidOperationException("late")),
            "Development");
        Func<Task> act = () => middleware.InvokeAsync(context.Object);
        await act.Should().ThrowAsync<InvalidOperationException>().ConfigureAwait(true);
    }
}
