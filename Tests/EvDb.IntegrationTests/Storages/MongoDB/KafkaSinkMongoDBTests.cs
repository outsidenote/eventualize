// Ignore Spelling: Mongo Aws

namespace EvDb.Core.Tests;
using Xunit.Abstractions;

[Trait("Kind", "Integration:sink")]
[Trait("DB", "MongoDB:sink")]
public class KafkaSinkMongoDBTests : KafkaSinkBaseTests
{
    public KafkaSinkMongoDBTests(ITestOutputHelper output) :
        base(output, StoreType.MongoDB)
    {
    }
}
