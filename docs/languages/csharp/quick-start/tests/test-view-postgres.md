---
layout: default
title: Stream + View on Postgres
nav_order: 2
parent: Tests
grand_parent: Quick Start
has_children: false
---

# Test Stream + View on Postgres

## Add Reference

```bash
dotnet add package EvDb.Adapters.Store.EvDbPostgres
dotnet add package EvDb.Adapters.Store.EvDbPostgresAdmin
```

### appsetting

Add connection string to appsetting.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "EvDbPostgresConnection": "Host=127.0.0.1;Port=5432;Database=test_db;User Id=test_user;Password=MasadNetunim12!@;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;"
  }
}
```

## Add Test Base Class

The base class will handle the concerns of creating and removing the db for the test

```cs
using EvDb.Core;
using Xunit.Abstractions;
#pragma warning disable S3881 // "IDisposable" should be implemented correctly

namespace EvDbQuickStart.Funds.IntegrationTests;

public abstract class BaseTests : IAsyncLifetime, IDisposable, IAsyncDisposable
{
    protected readonly ITestOutputHelper _output;
    protected abstract IEvDbStorageAdmin Admin { get; }

    protected BaseTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        await Admin.CreateEnvironmentAsync();
    }


    void IDisposable.Dispose()
    {
        DisposeAsync().Wait();
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await DisposeAsync();
    }

    public virtual async Task DisposeAsync()
    {
        await Admin.DestroyEnvironmentAsync();
    }

    ~BaseTests()
    {
        DisposeAsync().Wait();
    }
}
```

## Add Test Class

Write the test

```cs
using EvDb.Adapters.Store.Postgres;
using EvDb.Core;
using EvDbQuickStart.Funds.Events;
using EvDbQuickStart.Funds.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace EvDbQuickStart.Funds.IntegrationTests;

public sealed class PostgresTests :
{
    private const string CONN_STR_KEY = "EvDbPostgresConnection";
    private readonly IConfiguration _configuration;
    private readonly IEvDbFundsFactory _factory;
    private readonly string _tablePrefix = $"test_{Guid.NewGuid():N}";
    private readonly ILogger _logger;
    private readonly EvDbStorageContext _context;

    public PostgresTests(ITestOutputHelper output) : base(output)
    {
        var services = new ServiceCollection();

        // Register dependencies
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddProvider(new XUnitLoggerProvider(output));
        });

        _context = EvDbStorageContext.CreateWithEnvironment("tests", _tablePrefix, schema: "public");
        services.AddEvDb()
                        .AddFundsFactory(o => o.UsePostgresStoreForEvDbStream(), _context)
                        .DefaultSnapshotConfiguration(o => o.UsePostgresForEvDbSnapshot());

        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // Ensure your test project copies appsettings.json to output
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Register IConfiguration
        services.AddSingleton<IConfiguration>(_configuration);

        ServiceProvider sp = services.BuildServiceProvider();
        _configuration = sp.GetRequiredService<IConfiguration>();
        _factory = sp.GetRequiredService<IEvDbFundsFactory>();
        _logger = sp.GetRequiredService<ILogger<SqlServerTests>>();
        string connectionString = _configuration.GetConnectionString(CONN_STR_KEY)!;
        Admin = PostgresStorageAdminFactory.Create(_logger, connectionString, _context);
    }

    protected override IEvDbStorageAdmin Admin { get; }

    [Fact]
    public async Task AddEvents_CreateView_Test()
    {
        Guid streamId = Guid.NewGuid();

        IEvDbFunds stream = await _factory.GetAsync(streamId);
        var deposit = new DepositedEvent { Amount = 100, Attribution = "test 1.1" };
        await stream.AppendAsync(deposit);

        var withdraw = new WithdrawnEvent(30) { Attribution = "test 1.2" };
        await stream.AppendAsync(withdraw);

        await stream.StoreAsync();
        var balance = stream.Views.Balance;

        _output.WriteLine($"Balance: {balance.Funds}");

        Assert.Equal(70, balance.Funds);
    }
}
```
