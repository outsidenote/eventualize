namespace EvDb.Core.Tests;
using Xunit.Abstractions;

[Trait("Kind", "Integration")]
[Trait("DB", "Postgres")]
public class PostgresChangeStreamTests : ChangeStreamBaseTests
{
    public PostgresChangeStreamTests(ITestOutputHelper output) :
        base(output, StoreType.Postgres)
    {
    }
}