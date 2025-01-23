// Ignore Spelling: Sql

namespace EvDb.Core.Tests;
using Xunit.Abstractions;

public class PostgresStreamChaosTests : StreamTxSopeBaseTests
{
    public PostgresStreamChaosTests(ITestOutputHelper output) :
        base(output, StoreType.Postgres)
    {
    }
}