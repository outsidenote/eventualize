using EvDb.Core;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace CoreTests.EvDbRepositoryTests
{
    public sealed class TestStorageAdapter : IEvDbStorageAdapter
    {
        public Dictionary<string, EvDbStoredSnapshot<JsonElement>> Snapshots = new();
        public Dictionary<string, List<EvDbStoredEvent>> Events = new();

        #region StorePendingEvents

        internal void StorePendingEvents<T>(IEvDbAggregate<T> aggregate)
        {
            if (aggregate.EventsCount == 0)
                return;
            List<EvDbStoredEvent> storedEvents = [];
            string key = GetKeyValue(aggregate);
            if (!Events.TryGetValue(key, out var stream))
            {
                stream = [];
                Events.Add(key, stream);
            }

            foreach (var pendingEvent in aggregate.Events)
            {
                EvDbSnapshotCursor cursor = new(aggregate);
                EvDbStoredEvent storedEvent = new(pendingEvent, cursor);
                stream.Add(storedEvent);
            }
        }

        #endregion // StorePendingEvents

        #region StoreSnapshot

        private void StoreSnapshot<T>(IEvDbAggregate<T> aggregate) 
        {
            string key = GetKeyValue(aggregate.SnapshotId);
            var snapshotCursor = new EvDbSnapshotCursor(aggregate);
            JsonElement serializedSnapshot = JsonSerializer.SerializeToElement<T>(aggregate.State);
            EvDbStoredSnapshot<JsonElement> value = new(serializedSnapshot, snapshotCursor);
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

        internal static string GetKeyValue(EvDbStreamAddress streamId) => streamId.ToString();
        internal static string GetKeyValue(EvDbSnapshotId snapshotId) => snapshotId.ToString();
        internal static string GetKeyValue(IEvDbAggregate aggregate) => aggregate.StreamId.ToString();

        internal static string GetKeyValue<T>(EvDbAggregate<T> aggregate) where T : notnull, new() => aggregate.StreamId.ToString();

        #endregion // GetKeyValue

        #region IEvDbStorageAdapter Members

        Task<EvDbStoredSnapshot<T>?> IEvDbStorageAdapter.TryGetSnapshotAsync<T>(
                            EvDbSnapshotId snapshotId, CancellationToken cancellation)
        {
            return Task.Run(() =>
            {
                var key = GetKeyValue(snapshotId);
                if (!Snapshots.TryGetValue(key, out var value) || value == null)
                    return null;
                T? parsedShapshot = JsonSerializer.Deserialize<T>(value.State);
                var result = parsedShapshot != null ? new EvDbStoredSnapshot<T>(parsedShapshot, value.Cursor) : default(EvDbStoredSnapshot<T>);
                return result;
            });
        }

        async IAsyncEnumerable<IEvDbStoredEvent> IEvDbStorageAdapter.GetAsync(EvDbStreamCursor streamCursor, CancellationToken cancellation)
        {
            var (domain, aggregateTypeName, id, offset) = streamCursor;
            EvDbStreamAddress streamId = new(domain, aggregateTypeName, id);
            var key = GetKeyValue(streamId);
            if (!Events.TryGetValue(key, out List<EvDbStoredEvent>? events) || events == null)
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

        async Task IEvDbStorageAdapter.SaveAsync<T>(IEvDbAggregate<T> aggregate, bool storeSnapshot, JsonSerializerOptions? options, CancellationToken cancellation)
        {
            await Task.Run(() =>
            {
                StorePendingEvents<T>(aggregate);
                if (storeSnapshot)
                    StoreSnapshot(aggregate);
            });
        }



        Task<long> IEvDbStorageAdapter.GetLastOffsetAsync<T>(IEvDbAggregate<T> aggregate, CancellationToken cancellation)
        {
            string key = GetKeyValue(aggregate);
            if (!Events.TryGetValue(key, out var events))
            {
                return Task.FromResult((long)-1);
            }
            return Task.FromResult((long)events.Count - 1);
        }

        #endregion // IEvDbStorageAdapter Members

        #region Dispose

        ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;

        void IDisposable.Dispose() { }

        #endregion // Dispose
    }
}