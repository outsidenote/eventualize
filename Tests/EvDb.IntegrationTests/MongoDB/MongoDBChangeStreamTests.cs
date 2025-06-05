// Ignore Spelling: Sql Mongo

namespace EvDb.Core.Tests;
using Xunit.Abstractions;

[Trait("Kind", "Integration")]
[Trait("DB", "MongoDB")]
public class MongoDBChangeStreamTests : ChangeStreamBaseTests
{
    public MongoDBChangeStreamTests(ITestOutputHelper output) :
        base(output, StoreType.MongoDB)
    {
    }
}
