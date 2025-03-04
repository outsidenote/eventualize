// Ignore Spelling: Sql

namespace EvDb.Core.Tests;
using Xunit.Abstractions;

public class MongoDBStreamTests : StreamBaseTests
{
    public MongoDBStreamTests(ITestOutputHelper output) :
        base(output, StoreType.MongoDB)
    {
    }
}