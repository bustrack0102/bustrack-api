using Xunit;

namespace IntegrationTest;

/// <summary>One SQL container per assembly on the fixed port.</summary>
[CollectionDefinition("sddsql", DisableParallelization = true)]
public sealed class SddSqlSetup : ICollectionFixture<UnitTest.Common.MsSqlFixture>
{
}
