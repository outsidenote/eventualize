// Ignore Spelling: Sql

namespace EvDb.Core.Tests;
using Xunit.Abstractions;

public class PostgresStreamSimpleTests : StreamSimpleBaseTests
{
    public PostgresStreamSimpleTests(ITestOutputHelper output) :
        base(output, StoreType.Postgres)
    {
    }
}