using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Core.Aggregate;
using Core.Repository;

namespace CoreTests.RepositoryTests
{
    public class TestStorageAdapter : IStorageAdapter
    {
        public Dictionary<string, StoredSnapshotData<JsonDocument>> Snapshots = new();
        public Dictionary<string, List<Core.Event.Event>> Events = new();
        public Task<StoredSnapshotData<T>?> GetLatestSnapshot<T>(string aggregateTypeName, string id) where T : notnull, new()
        {
            var key = GetKeyValue(aggregateTypeName, id);
            StoredSnapshotData<JsonDocument>? value;
            if (!Snapshots.TryGetValue(key, out value) || value == null)
                return Task.FromResult(default(StoredSnapshotData<T>));
            T? parsedShapshot = JsonSerializer.Deserialize<T>(value.Snapshot);
            var result = parsedShapshot != null ? new StoredSnapshotData<T>(parsedShapshot, value.SnapshotSequenceId) : default(StoredSnapshotData<T>);
            return Task.FromResult(result);
        }

        public Task<List<Core.Event.Event>?> StorePendingEvents<T>(Aggregate<T> aggregate) where T : notnull, new()
        {
            List<Core.Event.Event>? eventsList;
            List<Core.Event.Event>? pendingEventsWithStoreTs = new();
            DateTime storeTs = DateTime.Now;
            foreach (var pendingEvent in aggregate.PendingEvents)
            {
                pendingEventsWithStoreTs.Add(new Core.Event.Event(pendingEvent, storeTs));
            }
            string key = GetKeyValue(aggregate);
            if (!Events.TryGetValue(key, out eventsList))
            {
                Events.Add(key, pendingEventsWithStoreTs);
            }
            else
            {
                eventsList = (List<Core.Event.Event>)eventsList.Concat(pendingEventsWithStoreTs);
            }
            return Task.FromResult(pendingEventsWithStoreTs ?? default);
        }

        public Task StoreSnapshot<T>(Aggregate<T> aggregate) where T : notnull, new()
        {
            string key = GetKeyValue(aggregate);
            long sequenceId = aggregate.LastStoredSequenceId + aggregate.PendingEvents.Count;
            JsonDocument serializedSnapshot = JsonDocument.Parse(JsonSerializer.Serialize<T>(aggregate.State));
            StoredSnapshotData<JsonDocument> value = new(serializedSnapshot, sequenceId);
            Snapshots.Add(key, value);
            return Task.FromResult(true);
        }

        public async Task<List<Core.Event.Event>?> Store<T>(Aggregate<T> aggregate, bool storeSnapshot) where T : notnull, new()
        {
            var events = await StorePendingEvents<T>(aggregate);
            if (storeSnapshot == true)
                await StoreSnapshot(aggregate);
            return events;
        }

        private static string GetKeyValue(string aggregateTypeName, string id) => $"{aggregateTypeName}_{id}";
        public static string GetKeyValue<T>(Aggregate<T> aggregate) where T : notnull, new() => GetKeyValue(aggregate.AggregateType.Name, aggregate.Id);

        public Task<List<Core.Event.Event>> GetStoredEvents(string aggregateTypeName, string id, long startSequenceId)
        {
            var key = GetKeyValue(aggregateTypeName, id);
            if (!Events.TryGetValue(key, out List<Core.Event.Event>? events) || events == null)
                return Task.FromResult(new List<Core.Event.Event>());
            try
            {
                return Task.FromResult(events.GetRange((int)startSequenceId, events.Count - (int)startSequenceId));
            }
            catch (ArgumentOutOfRangeException)
            {
                return Task.FromResult(new List<Core.Event.Event>());
            }
        }

        public Task Init()
        {
            return Task.FromResult(true);
        }
    }
}