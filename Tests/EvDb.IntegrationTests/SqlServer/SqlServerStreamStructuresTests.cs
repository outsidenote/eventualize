// Ignore Spelling: Sql

namespace EvDb.Core.Tests;
using Xunit.Abstractions;

public class SqlServerStreamStructuresTests : StreamStructuresBaseTests
{
    public SqlServerStreamStructuresTests(ITestOutputHelper output) :
        base(output, StoreType.SqlServer)
    {
    }

}