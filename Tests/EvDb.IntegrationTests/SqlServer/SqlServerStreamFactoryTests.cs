using Xunit.Abstractions;

namespace EvDb.Core.Tests;

public sealed class SqlServerStreamFactoryTests : StreamFactoryBaseTests
{
    public SqlServerStreamFactoryTests(ITestOutputHelper output) : base(output, StoreType.SqlServer)
    {
    }
}