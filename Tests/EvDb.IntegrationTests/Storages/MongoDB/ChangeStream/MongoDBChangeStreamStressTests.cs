// Ignore Spelling: Mongo

namespace EvDb.Core.Tests;
using Xunit.Abstractions;

[Trait("Kind", "Integration:stress")]
[Trait("DB", "MongoDB")]
public class MongoDBChangeStreamStressTests : ChangeStreamStressBaseTests
{
    public MongoDBChangeStreamStressTests(ITestOutputHelper output) :
        base(output, StoreType.MongoDB)
    {
    }
}
