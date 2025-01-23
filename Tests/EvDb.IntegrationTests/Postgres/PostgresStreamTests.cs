// Ignore Spelling: Sql

namespace EvDb.Core.Tests;
using Xunit.Abstractions;

public class PostgresStreamTests : StreamBaseTests
{
    public PostgresStreamTests(ITestOutputHelper output) :
        base(output, StoreType.Postgres)
    {
    }
}