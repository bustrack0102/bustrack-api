using Xunit;

namespace DatabaseTest;

/// <summary>Un contenedor SQL por assembly (puerto fijo MSSQL_PORT_FIXED).</summary>
[CollectionDefinition("sql", DisableParallelization = true)]
public sealed class SqlSetup : ICollectionFixture<UnitTest.Common.MsSqlFixture>
{
}
