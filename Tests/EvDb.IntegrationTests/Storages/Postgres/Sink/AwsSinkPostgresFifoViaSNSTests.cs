// Ignore Spelling:  Aws

namespace EvDb.Core.Tests;
using Xunit.Abstractions;

[Trait("Kind", "Integration:sink")]
[Trait("DB", "Postgres:sink")]
[Collection("Sink")]
public class AwsSinkPostgresFifoViaSNSTests: AwsSinkViaSNSBaseTests
{
    public AwsSinkPostgresFifoViaSNSTests(ITestOutputHelper output) :
        base(output, StoreType.Postgres)
    {
    }
}