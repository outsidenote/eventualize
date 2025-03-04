// Ignore Spelling: Sql

namespace EvDb.Core.Tests;
using Xunit.Abstractions;

public class MongoDBStreamSimpleTests : StreamSimpleBaseTests
{
    public MongoDBStreamSimpleTests(ITestOutputHelper output) :
        base(output, StoreType.MongoDB)
    {
    }
}