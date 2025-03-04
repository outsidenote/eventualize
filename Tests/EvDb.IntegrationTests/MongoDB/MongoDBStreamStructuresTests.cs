// Ignore Spelling: Sql

namespace EvDb.Core.Tests;
using Xunit.Abstractions;

public class MongoDBStreamStructuresTests : StreamStructuresBaseTests
{
    public MongoDBStreamStructuresTests(ITestOutputHelper output) :
        base(output, StoreType.MongoDB)
    {
    }
}