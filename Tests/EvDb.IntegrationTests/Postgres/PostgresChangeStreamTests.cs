// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using EvDb.Core;
using EvDb.Core.Adapters;
using System.Collections.Generic;
using Xunit.Abstractions;

[Trait("DB", "Postgres")]
public class PostgresChangeStreamTests : ChangeStreamBaseTests
{
    public PostgresChangeStreamTests(ITestOutputHelper output) :
        base(output, StoreType.Postgres)
    {
    }
}