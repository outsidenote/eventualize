namespace EvDb.Core.Tests;
using Xunit.Abstractions;

[Trait("Kind", "Integration:stress")]
[Trait("DB", "Postgres")]
public class PostgresChangeStreamStressTests : ChangeStreamStressBaseTests
{
    public PostgresChangeStreamStressTests(ITestOutputHelper output) :
        base(output, StoreType.Postgres)
    {
    }
}