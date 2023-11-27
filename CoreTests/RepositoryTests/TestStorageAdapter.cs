using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Core.Aggregate;
using Core.Repository;

namespace CoreTests.RepositoryTests
{
    public class TestStorageAdapter : IStorageAdapter
    {
        public Dictionary<string, GetLatestSnapshotData<JsonDocument>> Snapshots = new();
        public Dictionary<string, List<Core.Event.Event>> Events = new();
        public Task<GetLatestSnapshotData<T>?> GetLatestSnapshot<T>(string aggregateTypeName, string id) where T : notnull, new()
        {
            GetLatestSnapshotData<JsonDocument>? value;
            Snapshots.TryGetValue(id, out value);
            if (value == null) return Task.FromResult(default(GetLatestSnapshotData<T>));
            T? parsedShapshot = value.Snapshot != null ? JsonSerializer.Deserialize<T>(value.Snapshot) : default(T);
            var result = new GetLatestSnapshotData<T>(parsedShapshot, value.SnapshotSequenceId);
            return Task.FromResult(result ?? default(GetLatestSnapshotData<T>));
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
            aggregate.ClearPendingEvents();
            return Task.FromResult(pendingEventsWithStoreTs ?? default);
        }

        public Task StoreSnapshot<T>(Aggregate<T> aggregate) where T : notnull, new()
        {
            string key = GetKeyValue(aggregate);
            long sequenceId = aggregate.LastStoredSequenceId + aggregate.PendingEvents.Count;
            JsonDocument serializedSnapshot = JsonDocument.Parse(JsonSerializer.Serialize<Aggregate<T>>(aggregate));
            GetLatestSnapshotData<JsonDocument> value = new(serializedSnapshot, sequenceId);
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
        public static string GetKeyValue<T>(Aggregate<T> aggregate) where T : notnull, new() => GetKeyValue(nameof(aggregate.AggregateType), aggregate.Id);

        public Task<List<Core.Event.Event>> GetStoredEvents(long startSequenceId)
        {
            throw new NotImplementedException();
        }

        public Task Init()
        {
            return Task.FromResult(true);
        }
    }
}