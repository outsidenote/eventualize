// Ignore Spelling:  Aws

namespace EvDb.Core.Tests;
using Xunit.Abstractions;

[Trait("Kind", "Integration:sink")]
[Trait("DB", "Postgres:sink")]
public class KafkaSinkPostgresTests : KafkaSinkBaseTests
{
    public KafkaSinkPostgresTests(ITestOutputHelper output) :
        base(output, StoreType.Postgres)
    {
    }
}