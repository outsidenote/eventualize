// using Eventualize.Core.Abstractions.Stream;

// namespace Eventualize.Core;

// public class EventualizeAggregateType<T> where T : notnull, new()
// {
//     // [bnaya 2023-12-11] Consider what is the right data type (thread safe)
//     public Dictionary<string, EventualizeEventType> RegisteredEventTypes { get; private set; } = new();

//     // [bnaya 2023-12-11] Consider what is the right data type (thread safe)
//     public Dictionary<string, IFoldingFunction<T>> FoldingLogic = new();

//     public readonly string Name;

//     public readonly EventualizeStreamBaseAddress StreamBaseAddress;

//     public readonly int MinEventsBetweenSnapshots;

//     public EventualizeAggregateType(string name, EventualizeStreamBaseAddress streamBaseAddress)
//     {
//         Name = name;
//         StreamBaseAddress = streamBaseAddress;
//         MinEventsBetweenSnapshots = 0;
//     }

//     public EventualizeAggregateType(string name, EventualizeStreamBaseAddress streamBaseAddress, int minEventsBetweenSnapshots)
//     {
//         Name = name;
//         StreamBaseAddress = streamBaseAddress;
//         MinEventsBetweenSnapshots = minEventsBetweenSnapshots;
//     }

//     [Obsolete("Deprecated use aggregate directly")]
//     public EventualizeAggregate<T> CreateAggregate(string id)
//     {
//         return EventualizeAggregateFactory.Create(this, id);
//     }

//     [Obsolete("Deprecated use aggregate directly")]
//     public async Task<EventualizeAggregate<T>> CreateAggregateAsync(
//                                 string id,
//                                 IAsyncEnumerable<EventualizeEvent> events)
//     {
//         var result = await EventualizeAggregateFactory.CreateAsync(
//                                 this,
//                                 id,
//                                 MinEventsBetweenSnapshots,
//                                 events);
//         return result;
//     }

//     [Obsolete("Deprecated use aggregate directly")]
//     public async Task<EventualizeAggregate<T>> CreateAggregateAsync(
//                     string id,
//                     IAsyncEnumerable<EventualizeEvent> events,
//                     T snapshot,
//                     long lastStoredSequenceId)
//     {
//         var result = await EventualizeAggregateFactory.CreateAsync(
//                                     this,
//                                     id,
//                                     events,
//                                     snapshot,
//                                     lastStoredSequenceId);
//         return result;
//     }

//     public void AddEventType(EventualizeEventType eventType)
//     {
//         RegisteredEventTypes.Add(eventType.EventTypeName, eventType);
//     }

//     public void AddFoldingFunction(
//                             string eventTypeName,
//                             IFoldingFunction<T> foldingFunction)
//     {
//         EventualizeEventType? eventType;
//         if (!RegisteredEventTypes.TryGetValue(eventTypeName, out eventType))
//             throw new KeyNotFoundException($"Event type name {eventTypeName} was not found.");
//         FoldingLogic.Add(eventTypeName, foldingFunction);

//     }

//     public void AddEventType(
//                         EventualizeEventType eventType,
//                         IFoldingFunction<T> foldingFunction)
//     {
//         AddEventType(eventType);
//         AddFoldingFunction(eventType.EventTypeName, foldingFunction);
//     }

//     [Obsolete("deprecated", true)]
//     public T FoldEvents(T oldState, IEnumerable<EventualizeEvent> events)
//     {
//         T currentState = oldState;
//         foreach (var e in events)
//         {
//             currentState = FoldEvent(currentState, e);
//         }
//         return currentState;
//     }

//     public async Task<FoldingResult<T>> FoldEventsAsync(
//         IAsyncEnumerable<EventualizeEvent> events)
//     {
//         T state = new();
//         var result = await FoldEventsAsync(state, events);
//         return result;
//     }

//     public async Task<FoldingResult<T>> FoldEventsAsync(
//         T oldState,
//         IAsyncEnumerable<EventualizeEvent> events)
//     {
//         long count = 0;
//         T currentState = oldState;
//         await foreach (var e in events)
//         {
//             currentState = FoldEvent(currentState, e);
//             count++;
//         }
//         return new FoldingResult<T>(currentState, count);
//     }

//     public T FoldEvent(T oldState, EventualizeEvent someEvent)
//     {
//         T currentState = oldState;
//         IFoldingFunction<T>? foldingFunction;
//         if (!FoldingLogic.TryGetValue(someEvent.EventType, out foldingFunction)) throw new ArgumentNullException(nameof(someEvent));
//         currentState = foldingFunction.Fold(currentState, someEvent);
//         return currentState;
//     }
// }
