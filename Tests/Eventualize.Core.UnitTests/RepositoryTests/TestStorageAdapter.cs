using Eventualize.Core;
using Eventualize.Core.Abstractions.Stream;
using System.Collections.Immutable;
using System.Text.Json;

namespace CoreTests.RepositoryTests
{
    public sealed class TestStorageAdapter : IEventualizeStorageAdapter
    {
        public Dictionary<string, EventualizeStoredSnapshotData<JsonElement>> Snapshots = new();
        public Dictionary<string, List<EventualizeStoredEvent>> Events = new();

        #region StorePendingEvents

        internal void StorePendingEvents<T>(EventualizeAggregate<T> aggregate) where T : notnull, new()
        {
            if (aggregate.PendingEvents.Count == 0)
                return;
            List<EventualizeStoredEvent> storedEvents = [];
            string key = GetKeyValue(aggregate);
            long lastStoredSequenceId = -1;
            if (!Events.TryGetValue(key, out var stream))
            {
                stream = [];
                Events.Add(key, stream);
            }
            else
            {
                lastStoredSequenceId = stream.Count;
            }

            DateTime storeTs = DateTime.Now;
            foreach (var pendingEvent in aggregate.PendingEvents)
            {
                stream.Add(new EventualizeStoredEvent(pendingEvent, aggregate.StreamAddress, ++lastStoredSequenceId));
            }
        }

        #endregion // StorePendingEvents

        #region StoreSnapshot

        private void StoreSnapshot<T>(EventualizeAggregate<T> aggregate) where T : notnull, new()
        {
            string key = GetKeyValue(aggregate);
            long sequenceId = aggregate.LastStoredSequenceId + aggregate.PendingEvents.Count;
            JsonElement serializedSnapshot = JsonSerializer.SerializeToElement<T>(aggregate.State);
            EventualizeStoredSnapshotData<JsonElement> value = new(serializedSnapshot, sequenceId);
            if (!Snapshots.ContainsKey(key))
            {
                Snapshots.Add(key, value);
            }
            else
            {
                Snapshots[key] = value;
            }
        }

        #endregion // StoreSnapshot

        #region GetKeyValue

        internal static string GetKeyValue(EventualizeStreamAddress streamAddress) => streamAddress.ToString();
        internal static string GetKeyValue(EventualizeAggregate aggregate) => aggregate.StreamAddress.ToString();

        internal static string GetKeyValue<T>(EventualizeAggregate<T> aggregate) where T : notnull, new() => aggregate.StreamAddress.ToString();

        #endregion // GetKeyValue

        #region IEventualizeStorageAdapter Members

        Task<EventualizeStoredSnapshotData<T>?> IEventualizeStorageAdapter.TryGetSnapshotAsync<T>(
                            AggregateParameter parameter, CancellationToken cancellation)
        {
            var (id, aggregateTypeName) = parameter;
            EventualizeStreamAddress streamAddress = new("default", aggregateTypeName, id);
            var key = GetKeyValue(streamAddress);
            if (!Snapshots.TryGetValue(key, out var value) || value == null)
                return Task.FromResult(default(EventualizeStoredSnapshotData<T>));
            T? parsedShapshot = JsonSerializer.Deserialize<T>(value.Snapshot);
            var result = parsedShapshot != null ? new EventualizeStoredSnapshotData<T>(parsedShapshot, value.SnapshotSequenceId) : default(EventualizeStoredSnapshotData<T>);
            return Task.FromResult(result);
        }

        async IAsyncEnumerable<EventualizeStoredEvent> IEventualizeStorageAdapter.GetAsync(AggregateSequenceParameter parameter, CancellationToken cancellation)
        {
            var (id, aggregateTypeName, startSequenceId) = parameter;
            EventualizeStreamAddress streamAddress = new("default",aggregateTypeName,id);
            var key = GetKeyValue(streamAddress);
            if (!Events.TryGetValue(key, out List<EventualizeStoredEvent>? events) || events == null)
                yield break;
            //try
            //{
            var evts = events.GetRange((int)startSequenceId, events.Count - (int)startSequenceId);
            foreach (var e in evts)
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

        async Task IEventualizeStorageAdapter.SaveAsync<T>(EventualizeAggregate<T> aggregate, bool storeSnapshot, CancellationToken cancellation)
        {
            await Task.Run(() =>
            {
                StorePendingEvents<T>(aggregate);
                if (storeSnapshot)
                    StoreSnapshot(aggregate);
            });
        }

        Task<long> IEventualizeStorageAdapter.GetLastSequenceIdAsync<T>(EventualizeAggregate<T> aggregate, CancellationToken cancellationא)
        {
            string key = GetKeyValue(aggregate);
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