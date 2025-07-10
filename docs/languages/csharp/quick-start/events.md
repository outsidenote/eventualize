---
layout: default
title: Events
nav_order: 1
parent: Quick Start
grand_parent: Languages
has_children: false
---

# Events

## Get Started

### Create .NET Solution & Events Project

- Create an empty dotnet solution _(named: EvDbQuickStart)_
- Add New Class Libreary Project _(names: EvDbQuickStart.Funds.Events)_

### Add Reference

- Add NuGet reference to [`EvDb.Abstractions`](https://www.nuget.org/packages/EvDb.Abstractions)

  ```bash
  dotnet add package EvDb.Abstractions
  ```

## Add Events

1. Add a folder named `Types`
2. Add the following files into the `Types` folder

   ```cs
   // WithdrawnEvent.cs
   using EvDb.Core;

   namespace EvDbQuickStart.Funds.Events;

   [EvDbDefineEventPayload("withdrawn")]
   public readonly partial record struct WithdrawnEvent(double Amount); // Primary Ctor syntax
   ```

   ```cs
   // DepositedEvent.cs
   using EvDb.Core;

   namespace EvDbQuickStart.Funds.Events.Types;

   [EvDbDefineEventPayload("deposited")]
   public readonly partial record struct DepositedEvent
   {
       // Traditiional syntax
       public required double Amount { get; init; }
   }
   ```

   EvDb will use the attribute's parameter as the event type within the storage.
   It doesn't use the class/record name for it because it's might be changed during refactoring (the event type should be stable).

   > Best practice is to use readonly stuct for events (because it's immutable and GC friendly).
   > Yet you can replace `readonly partial record struct` with a `partial record`, `partial class` or `partial struct` if you will.
   > Note: the syntax doesn't matters, you can use traditional syntax or primary ctor, whatever you feel comftable with.

3. Build the solution
4. Make sure you're on the right path
   Select `WithdrawnEvent` and go to definition (F12).
   You should see a popup with 2 options
   ![alt text](/images/event-definitiongen.png){ width=700 }
   Select the one with `:IEvDbPayload`, this one wa generated for you by EvDB.
   You should see:

   ```cs
   [System.CodeDom.Compiler.GeneratedCode("EvDb.SourceGenerator","4.0.10.0")]
   [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]

   partial record struct WithdrawnEvent: IEvDbPayload
   {
       public static string PAYLOAD_TYPE => "withdrawn";
       [System.Text.Json.Serialization.JsonIgnore]
       string IEvDbPayload.PayloadType => PAYLOAD_TYPE;


   }
   ```

   > if you don't see it it's probobly becuse you forgaot to add the `partial` keyword.

5. Now it's time to create the events boundle  
   in order of defining which event-types can be part of a streams  
   (i.e. what kind of events can I append into the stream)

   - Add a `IAccountFundsEvents.cs` file under the project root.

   ```cs
   using EvDb.Core;
   using EvDbQuickStart.Funds.Events;
   using EvDbQuickStart.Funds.Events.Types;

   namespace EvDbQuickStart.Funds;

   [EvDbAttachEventType<DepositedEvent>]
   [EvDbAttachEventType<WithdrawnEvent>]
   public partial interface IAccountFundsEvents
   {

   }
   ```

   - ⚠ Don't forget the `partial` keyword.

   - Validation: goes to definition (F12)
     ![alt text](/images/IAccountFundsEvents.png){ width=700 }

     You should expect to see:

     ```cs
     [System.CodeDom.Compiler.GeneratedCode("EvDb.SourceGenerator","4.0.10.0")]

     partial interface IAccountFundsEvents: IEvDbEventTypes
     {
        ValueTask<IEvDbEventMeta> AppendAsync(EvDbQuickStart.Funds.Events.Types.DepositedEvent payload, string? capturedBy = null);
        ValueTask<IEvDbEventMeta> AppendAsync(EvDbQuickStart.Funds.Events.WithdrawnEvent payload, string? capturedBy = null);
     }
     ```

   - 🚀 EvDB is trying to take the boilerblate away

Continue to [Aggregate (Views)](aggregate)
