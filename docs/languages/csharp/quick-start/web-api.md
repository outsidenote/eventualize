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

[MongoDB](#mongodb) • [PostgreSQL](#postgres) • [SQL Server](#sql-server)

---

#### MongoDB

```bash
dotnet add package EvDb.Adapters.Store.EvDbMongoDB
```

---

#### Postgres

```bash
dotnet add package EvDb.Adapters.Store.EvDbMongoDB
```

#### Sql-Server

```bash
dotnet add package EvDb.Adapters.Store.EvDbMongoDB
```

### CLean up

Remove the `WeatherForecast` endpoint and entity.

### Register the factory

- Add `using EvDb.Core;`

1
1
1
1
1
1
1
1
1

1
1
1
1
1
1
1

1
1
1

---

- [Continue with Integration Tests](tests)
- [How to Set up local environment (databases)](docker-compose-dbs)
