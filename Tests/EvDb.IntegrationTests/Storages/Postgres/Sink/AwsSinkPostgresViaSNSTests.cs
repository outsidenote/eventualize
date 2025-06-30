// Ignore Spelling:  Aws

namespace EvDb.Core.Tests;
using Xunit.Abstractions;

[Trait("Kind", "Integration:sink")]
[Trait("DB", "Postgres:sink")]
public class AwsSinkPostgresViaSNSTests : AwsSinkViaSNSBaseTests
{
    public AwsSinkPostgresViaSNSTests(ITestOutputHelper output) :
        base(output, StoreType.Postgres)
    {
    }
}