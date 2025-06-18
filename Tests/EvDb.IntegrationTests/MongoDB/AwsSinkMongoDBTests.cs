// Ignore Spelling: Mongo Aws

namespace EvDb.Core.Tests;
using Xunit.Abstractions;

[Trait("Kind", "Integration")]
[Trait("DB", "MongoDB")]
[Trait("Feature", "Sink")]
public class AwsSinkMongoDBTests : AwsSinkBaseTests
{
    public AwsSinkMongoDBTests(ITestOutputHelper output) :
        base(output, StoreType.MongoDB)
    {
    }
}
