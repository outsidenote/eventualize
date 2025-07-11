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
dotnet add package EvDb.Adapters.Store.EvDbMongoDB
```

- Sql-Server

```bash
dotnet add package EvDb.Adapters.Store.EvDbMongoDB
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

---

- [Continue with Integration Tests](tests)
- [How to Set up local environment (databases)](docker-compose-dbs)
