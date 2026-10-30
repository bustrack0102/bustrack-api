using ApiSDD.Common;
using FluentAssertions;

namespace UnitTest.SDDUsuario;

/// <summary>nv-04 Unit: RowVersion wire codec.</summary>
public sealed class RowVersionCodecTests
{
    [Fact]
    public void RoundtripKeepsBytes()
    {
        var wire = RowVersionCodec.Encode([1, 2, 3]);

        RowVersionCodec.TryDecode(wire, out var back).Should().BeTrue();
        back.Should().Equal(1, 2, 3);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("!!!")]
    public void BadWireRejected(string? wire)
    {
        RowVersionCodec.TryDecode(wire, out _).Should().BeFalse();
    }

    [Fact]
    public void KnownVectorDecodes()
    {
        RowVersionCodec.TryDecode("AQ==", out var back).Should().BeTrue();
        back.Should().Equal(1);
    }
}
