// Ignore Spelling: Mongo Aws

namespace EvDb.Core.Tests;
using Xunit.Abstractions;

[Trait("Kind", "Integration:sink")]
[Trait("DB", "MongoDB:sink")]
[Collection("Sink")]
public class AwsSinkMongoDBFifoTests : AwsSinkFifoBaseTests
{
    public AwsSinkMongoDBFifoTests(ITestOutputHelper output) :
        base(output, StoreType.MongoDB)
    {
    }
}
