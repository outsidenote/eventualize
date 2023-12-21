---
layout: default
title: Events
nav_order: 2
parent: Learn More
---

# Events

<iframe width="560" height="315" src="https://www.youtube.com/embed/pGU7sRABo7M?si=nFXtiCOsOONL3eGK" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>

Events are facts that happened and are of interest to the software system.
In principal, anythings that is detected by the software system is an event.
Events can be detected from outside of the system, for example:

- User selected a product
- Third-party service sent an API request
  Events can be detected from within the system, for example:
- Free trial period ended
- User credit score updated
  As you can see, all the example are phrased in a past tense. This is because events are things that have already happened.

## Event Structure

In `Eventualize`, All events have the same structure:

| property name | data type | meaning                                                                |
| ------------- | --------- | ---------------------------------------------------------------------- |
| event_type    | string    | **required.** A description of the event (e.g. "user earned points")   |
| captured_at   | datetime  | **required.** The point in time in which the event was detected        |
| captured_by   | string    | **required.** The name of the service that detected the event          |
| payload       | json      | **required.** The data of the event (e.g. username, number of points)  |
| stored_at     | datetime  | _Optional._ Populated by `Eventualize` after the event has been stored |


Now that we understand events, we can learn how they are used to derive the system's state.
Hint: this is what **Aggregates** are for.
Let's proceed to the [Aggregates](aggregates) section!

