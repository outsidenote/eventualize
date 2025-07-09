---
layout: default
title: Outbox Pattern
nav_order: 3
parent: Main Mechanisms
grand_parent: Learn More
---

# Outbox Pattern 

## Overview

The **Outbox Pattern** is a messaging integration strategy that ensures consistency between a system’s state and the messages it emits. It is especially useful in distributed systems where achieving atomic operations across boundaries is complex.

Although it is **not part of the core Event Sourcing pattern**, the Outbox Pattern **works very well alongside it** by clearly separating:

- **Private domain events** – internal facts that describe what happened.
- **Public messages** – enriched or transformed representations meant for external systems.

## Event Sourcing Recap

In **Event Sourcing**, state changes are captured as a sequence of immutable domain events.  
These events are:

- **Internal** to the domain.
- Stored in an **append-only event store**.
- Used to **rebuild state** and drive internal behavior.

They are not necessarily suitable for direct exposure outside the system.

## Why Use the Outbox Pattern?

The Outbox Pattern is a valuable technique that complements Event Sourcing  
by providing a robust mechanism for publishing external messages.  
While not part of the core Event Sourcing pattern itself, 
it plays well with it to clearly distinguish between  
private events (domain's facts that are not exposed via a messaging system)  
and public messages that expose enriched facts to the outside world.

In systems like EventualizeDB (EvDB),  
the Outbox Pattern can be effectively built  
from the events themselves and their aggregations (views).

Here are the key reasons why the Outbox Pattern is  
beneficial, especially in the context of Event Sourcing:

- **Encapsulate internal logic** and avoid leaking internal details.  

- **Enrich or transform** events before they’re sent.  

- **Ensure reliability** by storing outbound messages in a durable store before dispatching.  

- **Enable internal event evolution without impacting external consumers**: This allows you to change the structure or content of your internal domain events (e.g., adding new fields or refactoring event types) without requiring external systems to adapt to those changes. The Outbox ensures a stable public contract.  

- **Decouple internal domain events from external messages**: This separation allows for different granularity and content between what's stored internally as a fact and what's exposed as a public notification.  

**Example: Deposit and Withdrawal Events vs. Account Balance Message**

Consider a banking system using Event Sourcing.  
Internally, you might have concise, granular domain events like DepositEvent and WithdrawalEvent.    

These internal events represent the immutable facts within your domain. However, an external system (e.g., a fraud detection service or a customer notification service) might not be interested in every granular deposit or withdrawal event. Instead, it might need to be notified about the current account balance after a significant transaction.  

This is where the Outbox Pattern comes into play. After processing a DepositEvent or WithdrawalEvent and updating the account's state (which involves calculating the new balance), an entry is added to the Outbox. This Outbox entry would represent the public message, which is an enriched version of the internal facts:

### Summary

Outbox messages are a type of projection, specifically tailored for messaging systems rather than for representing the application's current database state.

## Outbox in EvDb (EventtualizeDB)

In **EvDb (EventtualizeDB)**, the Outbox can be constructed from:

- The **event stream** – the raw domain history.
- The **aggregations (views)** – denormalized projections that reflect current state.

Using both, EvDb can generate **public messages** to populate the outbox. These messages are:

- Structured for external consumption.
- Filtered and enriched using context from views.
- Guaranteed to have a traceable origin in the domain events.

## Benefits

- ✅ **Reliability** – messages are persisted before dispatch.
- ✅ **Consistency** – state and outbound messages are aligned.
- ✅ **Separation of concerns** – internal events vs external messages.
- ✅ **Auditability** – messages are derived from immutable events.

## Summary

The **Outbox Pattern** is not part of the core **Event Sourcing** architecture, but it is a **natural and practical companion**. It enables you to:

- Keep domain events private and focused.
- Publish enriched, stable messages to the outside world.
- Build a clean separation between internal behavior and external integration.

In systems like **EvDb**, the outbox can be materialized directly from the event log and views, offering a robust, observable, and extensible approach to messaging.
