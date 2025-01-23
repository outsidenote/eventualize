using Xunit.Abstractions;

namespace EvDb.Core.Tests;

public sealed class PostgresStreamFactoryTests : StreamFactoryBaseTests
{
    public PostgresStreamFactoryTests(ITestOutputHelper output) : base(output, StoreType.Postgres)
    {
    }
}