// Ignore Spelling: Sql

using Xunit.Abstractions;

namespace EvDb.Core.Tests;

public sealed class SqlServerStressTests : StressBaseTests
{
    #region Ctor

    public SqlServerStressTests(ITestOutputHelper output) : base(output, StoreType.SqlServer)
    {
    }

    #endregion //  Ctor
}