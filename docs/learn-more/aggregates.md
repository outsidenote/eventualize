---
layout: default
title: Aggregates
nav_order: 4
parent: Learn More
---

## Aggregates

An **Aggregate** is a calculated entity state, derived from a single Stream..<br>
When you ask `EventualizeDB` to fetch the state of an entity, in principal what it does is fetching all the Entity's Stream's events and "folding" them one on top of the other to derive the current state.
Folding means taking a sequence of events and based on their content perform an aggregative calculation.

So when a service wants to create an aggregate, it needs to provide to `EventualizeDB` the identification of the relevant Stream in terms of **Stream Type** and **Stream ID**, and a **Folding Logic**, which is the mapping between an **Event Type** and the function that holds the desired calculation of its content. That means that you can read the same Stream using different folding logics, and derive different states from the same data!

<img src="../images/aggregate-naive-read-example.png" width="900"/>

Another important advantage of this approach is the it provides [strong consistency](https://en.wikipedia.org/wiki/Strong_consistency) when fetching an aggregate. Because immediately after storing an event, you'll be able to read it, as the Stream includes that event!

However in order to use it in large scale production systems there are 2 more important considerations:

1. **Read Time** - What if there are many events in an Stream? Wouldn't reading all of them in order to derive the state take a long time? For that we have [Snaphots](snapshots).
2. **Strong Consistency with Writes** - How can an service or application store an event, only if the Stream hasn't been updated since tha last time it was read? For that we have [Optimistic Concurrency Control](occ).

## The structure of an Aggregate

| property                      | data type                               | meaning                                                                                         |
| ----------------------------- | --------------------------------------- | ----------------------------------------------------------------------------------------------- |
| Stream Type                   | string                                  | **Required** The type of the Stream                                                             |
| Stream Id                     | string                                  | **Required** The identification of the Stream                                                   |
| State                         | object                                  | **Required** The current state the aggregate holds                                              |
| Folding Logic                 | dictionary<event type,folding function> | **Required** The folding logic that computed the state                                          |
| Pending Events                | collection<event>                       | **Required** Events that the aggregate holds and have not yet been stored in the Stream.        |
| Offset                        | long                                    | **Required** The last known Stream Offset by the Aggregate. Used for [OCC](main-mechanisms/occ) |
| Min. Events Between Snapshots | int                                     | **Required** Used for [Snapshots](main-mechanisms/snapshots)                                    |
