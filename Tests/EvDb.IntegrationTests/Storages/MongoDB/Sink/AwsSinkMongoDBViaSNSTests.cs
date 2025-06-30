// Ignore Spelling: Mongo Aws

namespace EvDb.Core.Tests;
using Xunit.Abstractions;

[Trait("Kind", "Integration:sink")]
[Trait("DB", "MongoDB:sink")]
public class AwsSinkMongoDBViaSNSTests : AwsSinkViaSNSBaseTests
{
    public AwsSinkMongoDBViaSNSTests(ITestOutputHelper output) :
        base(output, StoreType.MongoDB)
    {
    }
}
