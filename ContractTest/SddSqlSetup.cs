using Xunit;

namespace ContractTest;

/// <summary>One SQL container per assembly on the fixed port.</summary>
[CollectionDefinition("sddsqlcontract", DisableParallelization = true)]
public sealed class SddSqlSetup : ICollectionFixture<UnitTest.Common.MsSqlFixture>
{
}
