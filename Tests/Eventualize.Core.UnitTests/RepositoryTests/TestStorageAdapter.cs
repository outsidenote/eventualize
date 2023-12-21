using Eventualize.Core;
using System.Collections.Immutable;
using System.Text.Json;

namespace CoreTests.RepositoryTests
{
    public sealed class TestStorageAdapter : IEventualizeStorageAdapter
    {
        public Dictionary<string, EventualizeStoredSnapshotData<JsonDocument>> Snapshots = new();
        public Dictionary<string, List<EventualizeEvent>> Events = new();

        #region StorePendingEvents

        internal async Task<IImmutableList<EventualizeEvent>> StorePendingEvents<T>(EventualizeAggregate<T> aggregate) where T : notnull, new()
        {
            if (aggregate.PendingEvents.Count == 0)
                return ImmutableArray<EventualizeEvent>.Empty;
            List<EventualizeEvent> pendingEventsWithStoreTs = new();
            DateTime storeTs = DateTime.Now;
            foreach (var pendingEvent in aggregate.PendingEvents)
            {
                var e = pendingEvent with { StoredAt = storeTs };
                pendingEventsWithStoreTs.Add(e);
            }
            string key = GetKeyValue(aggregate);
            if (!Events.TryGetValue(key, out List<EventualizeEvent>? eventsList))
            {
                Events.Add(key, pendingEventsWithStoreTs);
            }
            else
            {
                eventsList.AddRange(pendingEventsWithStoreTs);
            }
            await Task.Yield();
            return pendingEventsWithStoreTs.ToImmutableArray();
        }

        #endregion // StorePendingEvents

        #region StoreSnapshot

        private Task StoreSnapshot<T>(EventualizeAggregate<T> aggregate) where T : notnull, new()
        {
            string key = GetKeyValue(aggregate);
            long sequenceId = aggregate.LastStoredSequenceId + aggregate.PendingEvents.Count;
            JsonDocument serializedSnapshot = JsonDocument.Parse(JsonSerializer.Serialize<T>(aggregate.State));
            EventualizeStoredSnapshotData<JsonDocument> value = new(serializedSnapshot, sequenceId);
            if (!Snapshots.TryGetValue(key, out var storedSnapshotData))
            {
                Snapshots.Add(key, value);
            }
            else
            {
                Snapshots[key] = value;
            }
            return Task.FromResult(true);
        }

        #endregion // StoreSnapshot

        #region GetKeyValue

        private static string GetKeyValue(string aggregateTypeName, string id) => $"{aggregateTypeName}_{id}";

        public static string GetKeyValue<T>(EventualizeAggregate<T> aggregate) where T : notnull, new() => GetKeyValue(aggregate.Type, aggregate.Id);

        #endregion // GetKeyValue

        #region IEventualizeStorageAdapter Members
        
        Task<EventualizeStoredSnapshotData<T>?> IEventualizeStorageAdapter.TryGetSnapshotAsync<T>(
                            AggregateParameter parameter, CancellationToken cancellation)
        {
            var (id, aggregateTypeName) = parameter;
            var key = GetKeyValue(aggregateTypeName, id);
            EventualizeStoredSnapshotData<JsonDocument>? value;
            if (!Snapshots.TryGetValue(key, out value) || value == null)
                return Task.FromResult(default(EventualizeStoredSnapshotData<T>));
            T? parsedShapshot = JsonSerializer.Deserialize<T>(value.Snapshot);
            var result = parsedShapshot != null ? new EventualizeStoredSnapshotData<T>(parsedShapshot, value.SnapshotSequenceId) : default(EventualizeStoredSnapshotData<T>);
            return Task.FromResult(result);
        }

        async IAsyncEnumerable<EventualizeEvent> IEventualizeStorageAdapter.GetAsync(AggregateSequenceParameter parameter, CancellationToken cancellation)
        {
            var (id, aggregateTypeName, startSequenceId) = parameter;
            var key = GetKeyValue(aggregateTypeName, id);
            if (!Events.TryGetValue(key, out List<EventualizeEvent>? events) || events == null)
                yield break;
            //try
            //{
            var evts = events.GetRange((int)startSequenceId, events.Count - (int)startSequenceId);
            foreach (EventualizeEvent e in evts)
            {
                await Task.Yield();
                yield return e;
            }
            //}
            //catch (ArgumentOutOfRangeException)
            //{
            //    yield break;
            //}
        }

        async Task<IImmutableList<EventualizeEvent>> IEventualizeStorageAdapter.SaveAsync<T>(EventualizeAggregate<T> aggregate, bool storeSnapshot, CancellationToken cancellation)
        {
            var events = await StorePendingEvents<T>(aggregate);
            if (storeSnapshot)
                await StoreSnapshot(aggregate);
            return events;
        }

        Task<long> IEventualizeStorageAdapter.GetLastSequenceIdAsync<T>(EventualizeAggregate<T> aggregate, CancellationToken cancellationא)
        {
            string key = GetKeyValue(aggregate.Type, aggregate.Id);
            if (!Events.TryGetValue(key, out var events))
            {
                return Task.FromResult((long)-1);
            }
            return Task.FromResult((long)events.Count - 1);
        }

        #endregion // IEventualizeStorageAdapter Members

        #region Dispose

        ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;

        void IDisposable.Dispose() { }

        #endregion // Dispose
    }
}