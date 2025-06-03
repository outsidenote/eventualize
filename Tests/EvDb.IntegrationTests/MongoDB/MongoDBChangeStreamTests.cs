// Ignore Spelling: Sql Mongo

namespace EvDb.Core.Tests;

using EvDb.Core;
using EvDb.Core.Adapters;
using System.Collections.Generic;
using Xunit.Abstractions;

[Trait("DB", "MongoDB")]
public class MongoDBChangeStreamTests : ChangeStreamBaseTests
{
    public MongoDBChangeStreamTests(ITestOutputHelper output) :
        base(output, StoreType.MongoDB)
    {
    }
}
