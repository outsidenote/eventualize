// Ignore Spelling: Sql

namespace EvDb.Core.Tests;
using Xunit.Abstractions;

public class SqlServerStreamSimpleTests : StreamSimpleBaseTests
{
    public SqlServerStreamSimpleTests(ITestOutputHelper output) :
        base(output, StoreType.SqlServer)
    {
    }

}