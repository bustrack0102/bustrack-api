using Xunit;

namespace SecurityTest;

/// <summary>One SQL container per assembly on the fixed port.</summary>
[CollectionDefinition("sddsqlsec", DisableParallelization = true)]
public sealed class SddSqlSetup : ICollectionFixture<UnitTest.Common.MsSqlFixture>
{
}
