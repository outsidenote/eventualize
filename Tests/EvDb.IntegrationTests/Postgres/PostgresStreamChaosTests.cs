﻿// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using EvDb.Core.Adapters;
using EvDb.Scenes;
using EvDb.UnitTests;
using System.Text.Json;
using Xunit.Abstractions;
using static EvDb.Adapters.Store.Postgres.EvDbPostgresStorageAdapterFactory;

public class PostgresStreamChaosTests : StreamTxSopeBaseTests
{
    public PostgresStreamChaosTests(ITestOutputHelper output) :
        base(output, StoreType.Postgres)
    {
    }
}