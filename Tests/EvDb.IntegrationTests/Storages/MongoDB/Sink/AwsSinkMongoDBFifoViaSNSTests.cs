// Ignore Spelling: Mongo Aws

namespace EvDb.Core.Tests;
using Xunit.Abstractions;

[Trait("Kind", "Integration:sink")]
[Trait("DB", "MongoDB:sink")]
public class AwsSinkMongoDBFifoViaSNSTests : AwsSinkFifoViaSNSBaseTests
{
    public AwsSinkMongoDBFifoViaSNSTests(ITestOutputHelper output) :
        base(output, StoreType.MongoDB)
    {
    }
}
