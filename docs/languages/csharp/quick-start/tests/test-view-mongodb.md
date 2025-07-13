---
layout: default
title: Stream + View on MongoDB
nav_order: 1
parent: Tests
grand_parent: Quick Start
has_children: false
---

# Test Stream + View on MongoDB

## Add Reference

```bash
dotnet add package EvDb.Adapters.Store.EvDbMongoDB
```

# appsetting

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
    "EvDbMongoDBConnection": "mongodb://localhost:27017"
  }
}
```

## Add Test Class

Write the test

```cs
using EvDb.Core;
using EvDbQuickStart.Funds.Events;
using EvDbQuickStart.Funds.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace EvDbQuickStart.Funds.IntegrationTests;

public sealed class MongoTests
{
    private readonly ITestOutputHelper _output;
    private readonly IConfiguration _configuration;
    private readonly IEvDbFundsFactory _factory;
    private readonly string _tablePrefix = $"test_{Guid.NewGuid():N}";

    public MongoTests(ITestOutputHelper output)
    {
        _output = output;
        var services = new ServiceCollection();

        // Register dependencies
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddProvider(new XUnitLoggerProvider(_output));
        });

        var context = EvDbStorageContext.CreateWithEnvironment("tests", _tablePrefix, schema: "default");
        services.AddEvDb()
                        .AddFundsFactory(o => o.UseMongoDBStoreForEvDbStream(), context)
                        .DefaultSnapshotConfiguration(o => o.UseMongoDBForEvDbSnapshot());

        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // Ensure your test project copies appsettings.json to output
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Register IConfiguration
        services.AddSingleton<IConfiguration>(_configuration);

        var sp = services.BuildServiceProvider();
        _configuration = sp.GetRequiredService<IConfiguration>();
        _factory = sp.GetRequiredService<IEvDbFundsFactory>();
    }

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

        Assert.Equal(70, balance.Funds);
    }
}
```
