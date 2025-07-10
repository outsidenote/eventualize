// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using EvDb.Core.Adapters;
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