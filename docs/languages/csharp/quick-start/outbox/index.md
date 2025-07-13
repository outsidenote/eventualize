---
layout: default
title: Outbox (Messaging)
nav_order: 9
parent: Languages
grand_parent: Learn More
has_children: true
---

# Outbox (Messaging)

Many times the events of event sourcing are confused with messafing and event driven architecture,  
While in fact events are data entities (facts) persisted into a storage.  
It true that those event can be drifted into messages over the wire.

## Event Sourcing vs. Messaging and Event-Driven Architecture

Event sourcing is often mistakenly conflated with messaging systems or event-driven architecture.  
In reality, **events in event sourcing are immutable data records—facts—that are persisted in a durable store** to represent state changes over time.

While it's true that these events _can_ be transformed into messages and transmitted over the wire (e.g., via a message broker), this is a **separate concern**.

> The core of event sourcing is about **persisting domain events as the source of truth**,  
> not about communication between services.

---

✅ **Event Sourcing**: Focuses on storing a sequence of domain events to reconstruct state.  
📨 **Messaging/Event-Driven Architecture**: Focuses on communication between services using events as messages.

## Bridging Event Sourcing and Messaging with the Outbox Pattern in EvDB

In event sourcing, domain events are **private facts**—internal representations of state transitions within an aggregate. These events are not meant to be exposed directly to external systems.

**EvDB**, an event-sourcing ORM, provides **first-class support for the Outbox Pattern**, enabling a seamless bridge between internal event persistence and external messaging.

### Native Projection and Enrichment

EvDB allows you to **project and enrich event data using the current aggregate state** before persisting:

- When an event is raised, EvDB gives access to the aggregate (view) at that moment.
- You can define the projection to **transform the private event into a public message**, enriching it with additional metadata, computed values, or contextual information.
- Both the **event** and the **outbox message** are stored **atomically** in the same transaction, ensuring consistency and reliability.

This approach ensures:

- **Encapsulation** of domain logic.
- **Controlled exposure** of data to external consumers.
- **Reliable delivery** of enriched messages via the outbox.

> 🔐 **Private Event (domain fact)**  
> 🧠 **Projection + Enrichment (using aggregate state)**  
> 📤 **Public Message (outbox)**

> ✅ With EvDB, the Outbox Pattern becomes a natural extension of event sourcing—enabling safe, expressive, and consistent communication across boundaries.

## Combine the Outbox Pattern with the events

Up to this point we defined our [events](../events), [vie (aggregation)](../aggregate) and a [stream factory](stream-factory)  
the next step is to define the outbox transformation.
