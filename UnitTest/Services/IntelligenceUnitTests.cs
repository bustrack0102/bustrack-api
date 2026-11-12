using ApiSDD.Dtos.Intelligence;
using ApiSDD.Services.Integrations;
using FluentAssertions;

namespace UnitTest.Services;

/// <summary>Etapa 3 Unit: Inferencia inercial determinista ML y anonimización sin PII.</summary>
public sealed class IntelligenceUnitTests
{
    [Fact]
    public void EvaluacionLocalClasificaBacheParadaYViaRegularConHashAnonimo()
    {
        var potholeSamples = new[]
        {
            new SensorMuestraDto(0, 0.1, 0.1, 9.8),
            new SensorMuestraDto(100, 2.5, 3.0, 20.1),
        };
        var severe = MlServiceClient.EvaluateDeterministicLocal("VJ-PII-TULA-1", "R-01", potholeSamples);
        severe.ClasificacionPrincipal.Should().Be("BACHE_SEVERO");
        severe.SeveridadMaxima.Should().Be(3);
        severe.ViajeAnonimoHash.Should().HaveLength(16);
        severe.ViajeAnonimoHash.Should().NotContain("VJ-PII-TULA-1");

        var stopSamples = Enumerable.Range(0, 6)
            .Select(i => new SensorMuestraDto(i * 100, 0.02, 0.01, 9.81))
            .ToArray();
        var stop = MlServiceClient.EvaluateDeterministicLocal("VJ-STOP-1", "R-01", stopSamples);
        stop.ClasificacionPrincipal.Should().Be("PARADA_DETECTADA");
        stop.SeveridadMaxima.Should().Be(1);
    }
}
