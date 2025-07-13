---
layout: default
title: Stream + View on SqlServer
nav_order: 3
parent: Tests
grand_parent: Quick Start
has_children: false
---

# Test Stream + View on SqlServer

## Add Reference

```bash
dotnet add package EvDb.Adapters.Store.EvDbSqlServer
dotnet add package EvDb.Adapters.Store.EvDbSqlServerAdmin
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
    "EvDbSqlServerConnection": "Data Source=127.0.0.1;User ID=sa;Password=MasadNetunim12!@;Initial Catalog=master;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;Connect Timeout=30;Max Pool Size=100;Min Pool Size=5;Pooling=true;"
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
using EvDb.Adapters.Store.SqlServer;
using EvDb.Core;
using EvDbQuickStart.Funds.Events;
using EvDbQuickStart.Funds.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace EvDbQuickStart.Funds.IntegrationTests;

public sealed class SqlServerTests : BaseTests
{
    private const string CONN_STR_KEY = "EvDbSqlServerConnection";
    private readonly IConfiguration _configuration;
    private readonly IEvDbFundsFactory _factory;
    private readonly string _tablePrefix = $"test_{Guid.NewGuid():N}";
    private readonly EvDbStorageContext _context;
    private readonly ILogger _logger;

    public SqlServerTests(ITestOutputHelper output) : base(output)
    {
        var services = new ServiceCollection();

        // Register dependencies
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddProvider(new XUnitLoggerProvider(output));
        });

        _context = EvDbStorageContext.CreateWithEnvironment("master", _tablePrefix, schema: "dbo");
        services.AddEvDb()
                        .AddFundsFactory(o => o.UseSqlServerStoreForEvDbStream(), _context)
                        .DefaultSnapshotConfiguration(o => o.UseSqlServerForEvDbSnapshot());

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
        Admin = SqlServerStorageAdminFactory.Create(_logger, connectionString, _context);
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
