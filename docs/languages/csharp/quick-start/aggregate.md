---
layout: default
title: Aggregate (Views)
nav_order: 2
parent: Quick Start
grand_parent: Languages
has_children: false
---

# Aggregate (Views)

After setting up the events, it is time to define our entity state (aggregate) that will take the form of a view.

In this phase we'll create a strong consistant state.

One of the unique offering of EvDB is the ability of having a strong consistant state.

One of the common push back event sourcing have is the question of how can I maintain a strong consistent state.
For example:  
How can I return the current balance in response of a withdrawal command?

Event though it opssible on other SDKs EvDB make it a first class citizen via the View entity.

A stream can have 0 to n views that reacts to each event appended into the stream.

A View build from a State (simple data entity) and a follding logic that define the transformation
of the state in response to the stream's events.

## Add a project for the state

Unlike the view that have a folding logic, the state is a simple data entity.
Therefore the state can consider as public data and should be defined in a shared component.

- Add New Class Library Project (_named: EvDbQuickStart.Funds.Abstractions_)
- Add Folder named `States`
- Add `Balance.cs` type into the `States` folder

```cs
namespace EvDbQuickStart.Funds.Abstractions;

public readonly record struct Balance(double Funds); // Primary Ctor syntax
```

## Add a project for the View

The view will define the folding logic of the state.

- Add New Class Library Project (_named: EvDbQuickStart.Funds.Views_)

### Add Reference

- Add NuGet reference to [`EvDb.Abstractions`](https://www.nuget.org/packages/EvDb.Abstractions)

  ````bash
  dotnet add package EvDb.Abstractions
  ```

  ````

- Add Project reference to `EvDbQuickStart.Funds.Abstractions`
  The view need to be aware of the state.

- Add Project reference to `EvDbQuickStart.Funds.Events`
  The view need folding logic should react to the events.

### Add the View logic

- Add `BalanceView.cs`
