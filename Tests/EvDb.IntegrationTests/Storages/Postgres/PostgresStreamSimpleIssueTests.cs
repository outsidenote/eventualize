// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using Xunit.Abstractions;

[Trait("Kind", "Integration")]
[Trait("DB", "Postgres")]
public class PostgresStreamSimpleIssueTests : StreamSimpleIssueBaseTests
{
    public PostgresStreamSimpleIssueTests(ITestOutputHelper output) :
        base(output, StoreType.Postgres)
    {
    }

}