using Xunit.Abstractions;

namespace EvDb.Core.Tests;

public sealed class PostgresStressTests : StressBaseTests
{
    #region Ctor

    public PostgresStressTests(ITestOutputHelper output) : base(output, StoreType.Postgres)
    {
    }

    #endregion //  Ctor
}