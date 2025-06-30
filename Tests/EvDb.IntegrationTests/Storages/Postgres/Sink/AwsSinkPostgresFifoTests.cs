// Ignore Spelling:  Aws

namespace EvDb.Core.Tests;
using Xunit.Abstractions;

[Trait("Kind", "Integration:sink")]
[Trait("DB", "Postgres:sink")]
[Collection("Sink")]
public class AwsSinkPostgresFifoTests: AwsSinkBaseTests
{
    public AwsSinkPostgresFifoTests(ITestOutputHelper output) :
        base(output, StoreType.Postgres)
    {
    }
}