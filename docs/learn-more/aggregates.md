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

When you ask `Eventualize` to read the state of an aggregate, in principal what it does is fetching all the aggregate's events and "folding" them one on top of the other to derive the current state.
Folding means taking a sequence of events and based on their content perform an aggregative calculation.

So when a service wants to read the state of an aggregate, it needs to provide to `Eventualize` the identification of the aggregate in terms of **Aggregate Type** and **Aggregate ID**, and a **Folding Logic**, which is the mapping between an **Event Type** and the function that holds the desired calculation of its content. That means that you can read the same aggregate using different folding logics, and derive different states from the same data!

<img src="../images/aggregate-naive-read-example.png" width="900"/>
