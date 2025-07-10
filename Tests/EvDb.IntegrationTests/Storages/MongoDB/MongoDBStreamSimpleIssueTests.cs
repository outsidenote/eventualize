// Ignore Spelling: Sql Mongo

namespace EvDb.Core.Tests;

using EvDb.Core.Adapters;
using EvDb.UnitTests;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit.Abstractions;

[Trait("Kind", "Integration")]
[Trait("DB", "MongoDB")]
public class MongoDBStreamSimpleIssueTests : StreamSimpleIssueBaseTests
{
    public MongoDBStreamSimpleIssueTests(ITestOutputHelper output) :
        base(output, StoreType.MongoDB)
    {
    }
}
