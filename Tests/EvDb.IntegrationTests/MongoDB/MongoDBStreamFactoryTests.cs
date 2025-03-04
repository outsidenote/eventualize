using Xunit.Abstractions;

namespace EvDb.Core.Tests;

public sealed class MongoDBStreamFactoryTests : StreamFactoryBaseTests
{
    public MongoDBStreamFactoryTests(ITestOutputHelper output) : base(output, StoreType.MongoDB)
    {
    }
}