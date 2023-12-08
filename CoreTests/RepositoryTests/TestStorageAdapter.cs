using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Core;
using Core.Aggregate;
using Core.Repository;

namespace CoreTests.RepositoryTests
{
    public class TestStorageAdapter : IStorageAdapter
    {
        public Dictionary<string, StoredSnapshotData<JsonDocument>> Snapshots = new();
        public Dictionary<string, List<Core.EventEntity>> Events = new();
        public Task<StoredSnapshotData<T>?> TryGetSnapshotAsync<T>(string aggregateTypeName, string id) where T : notnull, new()
        {
            var key = GetKeyValue(aggregateTypeName, id);
            StoredSnapshotData<JsonDocument>? value;
            if (!Snapshots.TryGetValue(key, out value) || value == null)
                return Task.FromResult(default(StoredSnapshotData<T>));
            T? parsedShapshot = JsonSerializer.Deserialize<T>(value.Snapshot);
            var result = parsedShapshot != null ? new StoredSnapshotData<T>(parsedShapshot, value.SnapshotSequenceId) : default(StoredSnapshotData<T>);
            return Task.FromResult(result);
        }

        public Task<List<Core.EventEntity>?> StorePendingEvents<T>(Aggregate<T> aggregate) where T : notnull, new()
        {
            if (aggregate.PendingEvents.Count == 0)
                return Task.FromResult(default(List<Core.EventEntity>));
            List<Core.EventEntity> pendingEventsWithStoreTs = new();
            DateTime storeTs = DateTime.Now;
            foreach (var pendingEvent in aggregate.PendingEvents)
            {
                var e = pendingEvent with { StoredAt = storeTs };
                pendingEventsWithStoreTs.Add(e);
            }
            string key = GetKeyValue(aggregate);
            if (!Events.TryGetValue(key, out List<Core.EventEntity>? eventsList))
            {
                Events.Add(key, pendingEventsWithStoreTs);
            }
            else
            {
                eventsList.AddRange(pendingEventsWithStoreTs);
            }
            return Task.FromResult(pendingEventsWithStoreTs ?? default);
        }

        public Task StoreSnapshot<T>(Aggregate<T> aggregate) where T : notnull, new()
        {
            string key = GetKeyValue(aggregate);
            long sequenceId = aggregate.LastStoredSequenceId + aggregate.PendingEvents.Count;
            JsonDocument serializedSnapshot = JsonDocument.Parse(JsonSerializer.Serialize<T>(aggregate.State));
            StoredSnapshotData<JsonDocument> value = new(serializedSnapshot, sequenceId);
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

        public async Task<List<Core.EventEntity>?> SaveAsync<T>(Aggregate<T> aggregate, bool storeSnapshot) where T : notnull, new()
        {
            var events = await StorePendingEvents<T>(aggregate);
            if (storeSnapshot == true)
                await StoreSnapshot(aggregate);
            return events;
        }

        private static string GetKeyValue(string aggregateTypeName, string id) => $"{aggregateTypeName}_{id}";
        public static string GetKeyValue<T>(Aggregate<T> aggregate) where T : notnull, new() => GetKeyValue(aggregate.AggregateType.Name, aggregate.Id);

        public Task<List<Core.EventEntity>> GetAsync(string aggregateTypeName, string id, long startSequenceId)
        {
            var key = GetKeyValue(aggregateTypeName, id);
            if (!Events.TryGetValue(key, out List<Core.EventEntity>? events) || events == null)
                return Task.FromResult(new List<Core.EventEntity>());
            try
            {
                return Task.FromResult(events.GetRange((int)startSequenceId, events.Count - (int)startSequenceId));
            }
            catch (ArgumentOutOfRangeException)
            {
                return Task.FromResult(new List<Core.EventEntity>());
            }
        }

        public Task Init()
        {
            return Task.FromResult(true);
        }

        public Task<long> GetLastSequenceIdAsync<T>(Aggregate<T> aggregate) where T : notnull, new()
        {
            string key = GetKeyValue(aggregate.AggregateType.Name, aggregate.Id);
            if (!Events.TryGetValue(key, out var events))
            {
                return Task.FromResult((long)-1);
            }
            return Task.FromResult((long)events.Count - 1);
        }
    }
}