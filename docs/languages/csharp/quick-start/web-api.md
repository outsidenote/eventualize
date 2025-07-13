---
layout: default
title: Web Api
nav_order: 4
parent: Quick Start
grand_parent: Languages
has_children: false
---

# Web API

By now you should have all the pices, [`Events`](events), [`Arrgeragion (View)`](aggregate) that expose a `state`, [`Stream Factory`](stream-factory) for creating a `stream`.

Now you have to pick your storage layers.
One storage layer for the stream, and another for the snapshots (optimized persistance of the view)

## Add a project for the Web-API

- Add New ASP.NET Core Web API Project (_named: EvDbQuickStart.Funds.WebAPI_)
  Remove the `Use controller` checkbox (not mandatory, it can work via controller).

### Add Reference

- Add Project reference to `EvDbQuickStart.Funds.Repositories`

Choose Store Adapter

- MongoDB

```bash
dotnet add package EvDb.Adapters.Store.EvDbMongoDB
```

- Postgres

```bash
dotnet add package EvDb.Adapters.Store.EvDbPostgres
```

- Sql-Server

```bash
dotnet add package EvDb.Adapters.Store.EvDbSqlServer
```

### CLean up

Remove the `WeatherForecast` endpoint and entity.

### appsetting

Add connection string to appsetting.

- MongoDB

```json
{
  "Logging": {...}
  },
  "ConnectionStrings": {
    "EvDbMongoDBConnection": "mongodb://localhost:27017"
  }
}
```

- Postgres

```json
{
  "Logging": {...}
  },
  "ConnectionStrings": {
    "EvDbPostgresConnection": "Host=127.0.0.1;Port=5432;Database=test_db;User Id=test_user;Password=MasadNetunim12!@;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;"
  }
}
```

- Sql-Server

```json
{
  "Logging": {...}
  },
  "ConnectionStrings": {
    "EvDbSqlServerConnection": "Data Source=127.0.0.1;User ID=sa;Password=MasadNetunim12!@;Initial Catalog=master;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;Connect Timeout=30;Max Pool Size=100;Min Pool Size=5;Pooling=true;"
  }
}
```

### Register the factory

#### Using

- Add `using EvDb.Core;`

### Register Dependency Injection

- MongoDB

```cs
var context = EvDbStorageContext.CreateWithEnvironment("tests", "evdb-quick-start", schema: "default");
builder.Services.AddEvDb()
                .AddFundsFactory(o => o.UseMongoDBStoreForEvDbStream(), context)
                .DefaultSnapshotConfiguration(o => o.UseMongoDBForEvDbSnapshot());
```

- Postgres

```cs
var context = EvDbStorageContext.CreateWithEnvironment("tests", "evdb-quick-start", schema: "public");
builder.Services.AddEvDb()
                .AddFundsFactory(o => o.UsePostgresStoreForEvDbStream(), context)
                .DefaultSnapshotConfiguration(o => o.UsePostgresForEvDbSnapshot());
```

- Sql-server

```cs
var context = EvDbStorageContext.CreateWithEnvironment("master", "evdb-quick-start", schema: "dbo");
builder.Services.AddEvDb()
                .AddFundsFactory(o => o.UseSqlServerStoreForEvDbStream(), context)
                .DefaultSnapshotConfiguration(o => o.UseSqlServerForEvDbSnapshot());
```

#### Create Request object

```cs
namespace EvDbQuickStart.Funds.WebAPI;

public enum OperationType
{
    Deposit,
    Withdraw
}

public record FunRequest(OperationType Operation, Guid AccountId, int Amount)
{
    public string? Attribution { get; init; }
}
```

### Add Endpoint

```cs
app.MapGet("/quick-start/{accountId}", async (IEvDbFundsFactory factory, int accountId) =>
{
    IEvDbFunds stream = await factory.GetAsync(accountId);
    var balance = stream.Views.Balance;
    return balance;
})
.WithOpenApi();

app.MapPost("/quick-start/", async (IEvDbFundsFactory factory, FunRequest request) =>
{
    // Consider to move this logic to a service layer
    IEvDbFunds stream = await factory.GetAsync(request.AccountId);
    if(request.Operation == OperationType.Deposit)
    {
        var deposit = new DepositedEvent { Amount = request.Amount, Attribution = request.Attribution };
        await stream.AppendAsync(deposit);
    }
    else if(request.Operation == OperationType.Withdraw)
    {
        var deposit = new WithdrawnEvent(request.Amount){ Attribution = request.Attribution };
        await stream.AppendAsync(deposit);
    }
    await stream.StoreAsync();
    var balance = stream.Views.Balance;
    return balance;
})
.WithOpenApi();
```

Set up the environment, [create database](create-database) and you're ready to go.

---

- [Create database](create-database)
- [Continue with Integration Tests](tests)
- [How to Set up local environment (databases)](docker-compose-dbs)
