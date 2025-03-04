// Ignore Spelling: Sql

namespace EvDb.Core.Tests;
using Xunit.Abstractions;

public class MongoDBStreamChaosTests : StreamTxSopeBaseTests
{
    public MongoDBStreamChaosTests(ITestOutputHelper output) :
        base(output, StoreType.MongoDB)
    {
    }
}