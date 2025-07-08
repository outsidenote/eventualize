# What is Event Sourcing

Event Sourcing is a software architectural pattern  
in which state changes are logged as a sequence of events.  
Instead of storing just the current state of the data,  
every change to the state is captured s a fact  
in an event object and stored in an append-only sequence  
called the event store.

This pattern provides a reliable audit trail,  
enables time-traveling (e.g., what was the state at a given time),  
and supports rebuilding state by replaying events.

## Key Concepts
  
- **Command**: A request to perform an action that changes the state.
- **Event**: A record of something that has happened in the system.
- **Event Store**: A durable storage of all events in the order they occurred.
- **Projection (Read Model)**: A view of the data built by processing events, optimized for queries.
- **Query**: A request to retrieve data from the read model.

## Benefits

- Full audit trail of all changes  
- Easy to debug and trace issues  
- Enables time-travel queries and state replay  
- Supports CQRS (Command Query Responsibility Segregation)  
- Flexible evolution  
  - Build new projections from past events  
  - Easier schema changes over time  
- Ideal for AI training (facts over time)

## Use Cases

- Financial systems requiring audit logs  
- Systems with complex business rules  
- Applications needing high traceability  
- CQRS-based architectures  
- Event-driven microservices  
- Evolving schemas with minimal disruption

