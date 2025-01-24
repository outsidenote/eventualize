// Ignore Spelling: Sql

namespace EvDb.Core.Tests;
using Xunit.Abstractions;

public class SqlServerStreamChaosTests : StreamTxSopeBaseTests
{
    public SqlServerStreamChaosTests(ITestOutputHelper output) :
        base(output, StoreType.SqlServer)
    {
    }

}