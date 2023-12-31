// using EvDb.Core;

// namespace EvDb.Core;

// public class EvDbAggregateType<T> where T : notnull, new()
// {
//     // [bnaya 2023-12-11] Consider what is the right data type (thread safe)
//     public Dictionary<string, EvDbEventType> RegisteredEventTypes { get; private set; } = new();

//     // [bnaya 2023-12-11] Consider what is the right data type (thread safe)
//     public Dictionary<string, IFoldingFunction<T>> FoldingLogic = new();

//     public readonly string Name;

//     public readonly EvDbStreamBaseAddress StreamBaseAddress;

//     public readonly int MinEventsBetweenSnapshots;

//     public EvDbAggregateType(string name, EvDbStreamBaseAddress streamBaseAddress)
//     {
//         Name = name;
//         StreamBaseAddress = streamBaseAddress;
//         MinEventsBetweenSnapshots = 0;
//     }

//     public EvDbAggregateType(string name, EvDbStreamBaseAddress streamBaseAddress, int minEventsBetweenSnapshots)
//     {
//         Name = name;
//         StreamBaseAddress = streamBaseAddress;
//         MinEventsBetweenSnapshots = minEventsBetweenSnapshots;
//     }

//     [Obsolete("Deprecated use aggregate directly")]
//     public EvDbAggregate<T> CreateAggregate(string id)
//     {
//         return EvDbAggregateFactory.Create(this, id);
//     }

//     [Obsolete("Deprecated use aggregate directly")]
//     public async Task<EvDbAggregate<T>> CreateAggregateAsync(
//                                 string id,
//                                 IAsyncEnumerable<EvDbEvent> events)
//     {
//         var result = await EvDbAggregateFactory.CreateAsync(
//                                 this,
//                                 id,
//                                 MinEventsBetweenSnapshots,
//                                 events);
//         return result;
//     }

//     [Obsolete("Deprecated use aggregate directly")]
//     public async Task<EvDbAggregate<T>> CreateAggregateAsync(
//                     string id,
//                     IAsyncEnumerable<EvDbEvent> events,
//                     T snapshot,
//                     long lastStoredSequenceId)
//     {
//         var result = await EvDbAggregateFactory.CreateAsync(
//                                     this,
//                                     id,
//                                     events,
//                                     snapshot,
//                                     lastStoredSequenceId);
//         return result;
//     }

//     public void AddEventType(EvDbEventType eventType)
//     {
//         RegisteredEventTypes.Add(eventType.EventTypeName, eventType);
//     }

//     public void AddFoldingFunction(
//                             string eventTypeName,
//                             IFoldingFunction<T> foldingFunction)
//     {
//         EvDbEventType? eventType;
//         if (!RegisteredEventTypes.TryGetValue(eventTypeName, out eventType))
//             throw new KeyNotFoundException($"Event type name {eventTypeName} was not found.");
//         FoldingLogic.Add(eventTypeName, foldingFunction);

//     }

//     public void AddEventType(
//                         EvDbEventType eventType,
//                         IFoldingFunction<T> foldingFunction)
//     {
//         AddEventType(eventType);
//         AddFoldingFunction(eventType.EventTypeName, foldingFunction);
//     }

//     [Obsolete("deprecated", true)]
//     public T FoldEvents(T oldState, IEnumerable<EvDbEvent> events)
//     {
//         T currentState = oldState;
//         foreach (var e in events)
//         {
//             currentState = FoldEvent(currentState, e);
//         }
//         return currentState;
//     }

//     public async Task<FoldingResult<T>> FoldEventsAsync(
//         IAsyncEnumerable<EvDbEvent> events)
//     {
//         T state = new();
//         var result = await FoldEventsAsync(state, events);
//         return result;
//     }

//     public async Task<FoldingResult<T>> FoldEventsAsync(
//         T oldState,
//         IAsyncEnumerable<EvDbEvent> events)
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

//     public T FoldEvent(T oldState, EvDbEvent someEvent)
//     {
//         T currentState = oldState;
//         IFoldingFunction<T>? foldingFunction;
//         if (!FoldingLogic.TryGetValue(someEvent.EventType, out foldingFunction)) throw new ArgumentNullException(nameof(someEvent));
//         currentState = foldingFunction.Fold(currentState, someEvent);
//         return currentState;
//     }
// }
