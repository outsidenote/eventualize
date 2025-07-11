---
layout: default
title: Stream Factory
nav_order: 3
parent: Quick Start
grand_parent: Languages
has_children: false
---

# Stream Factory

The stream factory is where everything materialized into a functional event sourcing solution.
It defined the stream type, attached to events bundle and views.

## Add a project for the stream factory

The stream factory is very match an event sourcing ORM therefore we'll put it into a repository project.

- Add New Class Library Project (_named: EvDbQuickStart.Funds.Repositories_)

### Add Reference

- Add Project reference to `EvDbQuickStart.Funds.Views`

### Add the factory type

- Add `FundsFactory.cs`

```cs
using EvDb.Core;
using EvDbQuickStart.Funds.Views;

namespace EvDbQuickStart.Funds.Repositories;


[EvDbAttachView<BalanceView>]
[EvDbStreamFactory<IAccountFundsEvents>("ev-bank:funds")]
public partial class FundsFactory
{
}
```

That's all you're ready to go 🚀.

---

- [Continue with a Service (Web API)](web-api)
- [Continue with Integration Tests](tests)
- [How to Set up local environment (databases)](docker-compose-dbs)
