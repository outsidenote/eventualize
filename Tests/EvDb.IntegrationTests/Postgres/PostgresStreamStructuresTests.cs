// Ignore Spelling: Sql

namespace EvDb.Core.Tests;
using Xunit.Abstractions;

public class PostgresStreamStructuresTests : StreamStructuresBaseTests
{
    public PostgresStreamStructuresTests(ITestOutputHelper output) :
        base(output, StoreType.Postgres)
    {
    }
}