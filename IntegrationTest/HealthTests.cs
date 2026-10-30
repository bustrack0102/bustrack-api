using System.Net;
using FluentAssertions;
using UnitTest.Common;

namespace IntegrationTest;

/// <summary>NV-01 Integration/Health 2: liveness siempre 200; ready 503 sin DB.</summary>
public sealed class HealthTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public HealthTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task LivenessSiempre200()
    {
        var client = _factory.CreateClient();
        var response = await TestHttp.GetAsync(client, "/health").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Ready503SinDb()
    {
        var client = _factory.CreateClient();
        var response = await TestHttp.GetAsync(client, "/health/ready").ConfigureAwait(true);

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }
}
