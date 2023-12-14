---
layout: default
title: Aggregates
nav_order: 3
parent: Learn More
has_children: true
---

# Aggregates

Events that are captured, stored in **Aggregates**.
As their name implies, they aggregate in once place, events that belong to the same entity.
In order to easily locate an aggregate, it has an **Aggregate Type** and an **Aggregate Id**.
The combination of both need to be unique across the system.

For example, you can define an aggregate of type `user` with an id of `john`.
Now you can store all events that belong to the user "John" in that aggregates.

<img src="../images/aggregate-example.png" width="500"/>