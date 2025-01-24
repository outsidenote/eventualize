// Ignore Spelling: Sql

namespace EvDb.Core.Tests;
using Xunit.Abstractions;

public class SqlServerStreamTests : StreamBaseTests
{
    public SqlServerStreamTests(ITestOutputHelper output) :
        base(output, StoreType.SqlServer)
    {
    }

}