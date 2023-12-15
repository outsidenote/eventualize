---
layout: default
title: Local Aggregate
nav_order: 2
parent: Aggregates
grand_parent: Learn More
---

# Local Aggregate
Up until now, we've discussed the aggregate as the place where events of the same entity are stored in.<br>
This is the stored aggregate.<br>
However for code to effectively execute, it needs to have a local representation of the aggregate, a **local aggregate**, which can differ, throughout the code's execution, from the stored aggregate.

Let's see how it looks like and how it is used.

## The structure of a local aggregate
A local aggregate has the following:
* **Aggregate Type** - The type of the aggregate
* **Aggregate Id** - The identification of the aggregate
* **State** - The current state the aggregate holds
* **Folding Logic** - The folding logic that computed the state
* **Pending Events** - Events that the local aggregate holds that have not yet been stored in the stored aggregate.

## Creation of local aggregate from a stored aggregate
In order to create a local aggregate, the application code provides to the `Eventualize` SDK:
1. The aggregate type
2. The aggregate ID
3. Folding Logic

Then `Eventualize` locates the relevant stored aggregate, and the latest snapshot of the provided folding logic (if it exists). `Eventualize` folds the events since the latest snapshot (if it exists) and retruns a local aggregate with the following values:
* **Aggregate Type** - As provided by the application
* **Aggregate Id** - As provided by the application
* **State** - The dervied state from folding the events in the stored aggregate
* **Folding Logic** - As provided by the application
* **Pending Events** - An empty collection

Here is an illustration of the process:
