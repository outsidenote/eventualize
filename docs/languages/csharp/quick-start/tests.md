---
layout: default
title: Tests
nav_order: 4
parent: Quick Start
grand_parent: Languages
has_children: false
---

# Tests

## Add Integration Tests project

- Add New xUnit Test Project (_named: EvDbQuickStart.Funds.IntegrationTests_)

### Add Reference

- Add Project reference to `EvDbQuickStart.Funds.Repositories`

Add reference to Cocona

```bash
dotnet add package Cocona
```

Add reference to FakeItEasy

```bash
dotnet add package FakeItEasy
```

Choose Store Adapter

- MongoDB

```bash
dotnet add package EvDb.Adapters.Store.EvDbMongoDB
```

- Postgres

```bash
dotnet add package EvDb.Adapters.Store.EvDbPostgres
dotnet add package EvDb.Adapters.Store.EvDbPostgresAdmin
```

- Sql-Server

```bash
dotnet add package EvDb.Adapters.Store.EvDbSqlServer
dotnet add package EvDb.Adapters.Store.EvDbSqlServerAdmin
```

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

---

- [Continue with a Service (Web API)](web-api)
- [How to Set up local environment (databases)](docker-compose-dbs)
