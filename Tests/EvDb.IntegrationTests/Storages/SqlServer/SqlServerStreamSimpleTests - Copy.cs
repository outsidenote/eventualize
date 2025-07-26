// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using Xunit.Abstractions;

[Trait("Kind", "Integration")]
[Trait("DB", "SqlServer")]
public class SqlServerStreamSimpleIssueTests : StreamSimpleIssueBaseTests
{
    public SqlServerStreamSimpleIssueTests(ITestOutputHelper output) :
        base(output, StoreType.SqlServer)
    {
    }
}