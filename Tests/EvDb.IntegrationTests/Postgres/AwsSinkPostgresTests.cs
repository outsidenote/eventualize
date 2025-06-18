// Ignore Spelling:  Aws

namespace EvDb.Core.Tests;
using Xunit.Abstractions;

[Trait("Kind", "Integration")]
[Trait("DB", "Postgres")]
[Trait("Feature", "Sink")]
public class AwsSinkPostgresTests : AwsSinkBaseTests
{
    public AwsSinkPostgresTests(ITestOutputHelper output) :
        base(output, StoreType.Postgres)
    {
    }
}