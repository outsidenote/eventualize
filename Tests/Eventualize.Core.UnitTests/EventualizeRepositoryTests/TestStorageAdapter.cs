using Eventualize.Core;
using Eventualize.Core;
using System.Collections.Immutable;
using System.Text.Json;

namespace CoreTests.EventualizeRepositoryTests
{
    public sealed class TestStorageAdapter : IEventualizeStorageAdapter
    {
        public Dictionary<string, EventualizeStoredSnapshot<JsonElement>> Snapshots = new();
        public Dictionary<string, List<EventualizeStoredEvent>> Events = new();

        #region StorePendingEvents

        internal void StorePendingEvents<T>(EventualizeAggregate<T> aggregate) where T : notnull, new()
        {
            if (aggregate.PendingEvents.Count == 0)
                return;
            List<EventualizeStoredEvent> storedEvents = [];
            string key = GetKeyValue(aggregate);
            long lastStoredOffset = -1;
            if (!Events.TryGetValue(key, out var stream))
            {
                stream = [];
                Events.Add(key, stream);
            }
            else
            {
                lastStoredOffset = stream.Count;
            }

            DateTime storeTs = DateTime.Now;
            foreach (var pendingEvent in aggregate.PendingEvents)
            {
                stream.Add(new EventualizeStoredEvent(pendingEvent, aggregate.StreamUri, ++lastStoredOffset));
            }
        }

        #endregion // StorePendingEvents

        #region StoreSnapshot

        private void StoreSnapshot<T>(EventualizeAggregate<T> aggregate) where T : notnull, new()
        {
            string key = GetKeyValue(aggregate.SnapshotUri);
            var snapshotCursor = new EventualizeSnapshotCursor(aggregate);
            JsonElement serializedSnapshot = JsonSerializer.SerializeToElement<T>(aggregate.State);
            EventualizeStoredSnapshot<JsonElement> value = new(serializedSnapshot, snapshotCursor);
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

        internal static string GetKeyValue(EventualizeStreamUri streamUri) => streamUri.ToString();
        internal static string GetKeyValue(EventualizeSnapshotUri snapshotUri) => snapshotUri.ToString();
        internal static string GetKeyValue(EventualizeAggregate aggregate) => aggregate.StreamUri.ToString();

        internal static string GetKeyValue<T>(EventualizeAggregate<T> aggregate) where T : notnull, new() => aggregate.StreamUri.ToString();

        #endregion // GetKeyValue

        #region IEventualizeStorageAdapter Members

        Task<EventualizeStoredSnapshot<T>?> IEventualizeStorageAdapter.TryGetSnapshotAsync<T>(
                            EventualizeSnapshotUri snapshotUri, CancellationToken cancellation)
        {
            return Task.Run(() =>
            {
                var key = GetKeyValue(snapshotUri);
                if (!Snapshots.TryGetValue(key, out var value) || value == null)
                    return null;
                T? parsedShapshot = JsonSerializer.Deserialize<T>(value.State);
                var result = parsedShapshot != null ? new EventualizeStoredSnapshot<T>(parsedShapshot, value.Cursor) : default(EventualizeStoredSnapshot<T>);
                return result;
            });
        }

        async IAsyncEnumerable<EventualizeStoredEvent> IEventualizeStorageAdapter.GetAsync(EventualizeStreamCursor streamCursor, CancellationToken cancellation)
        {
            var (domain, aggregateTypeName, id, offset) = streamCursor;
            EventualizeStreamUri streamUri = new(domain, aggregateTypeName, id);
            var key = GetKeyValue(streamUri);
            if (!Events.TryGetValue(key, out List<EventualizeStoredEvent>? events) || events == null)
                yield break;
            //try
            //{
            var evts = events.GetRange((int)offset, events.Count - (int)offset);
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

        Task<long> IEventualizeStorageAdapter.GetLastOffsetAsync<T>(EventualizeAggregate<T> aggregate, CancellationToken cancellationא)
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