// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using EvDb.Core.Adapters;
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