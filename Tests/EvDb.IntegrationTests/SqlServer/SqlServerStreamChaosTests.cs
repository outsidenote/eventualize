// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using EvDb.Core.Adapters;
using EvDb.Scenes;
using EvDb.UnitTests;
using System.Text.Json;
using Xunit.Abstractions;
using static EvDb.Adapters.Store.SqlServer.EvDbSqlServerStorageAdapterFactory;

public class SqlServerStreamChaosTests : StreamChaosBaseTests
{
    public SqlServerStreamChaosTests(ITestOutputHelper output) :
        base(output, StoreType.SqlServer)
    {
    }

}