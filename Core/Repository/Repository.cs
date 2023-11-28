using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.AggregateType;
using Core.Aggregate;

namespace Core.Repository
{
    public class Repository
    {
        IStorageAdapter StorageAdapter;
        public Repository(IStorageAdapter storageAdapter)
        {
            StorageAdapter = storageAdapter;
        }

        public async Task Init()
        {
            await StorageAdapter.Init();
        }

        private static long GetNextSequenceId(long? sequenceId)
        {
            if (sequenceId == null) return 0;
            return (long)sequenceId + 1;
        }

        public async Task<Aggregate<T>> Get<T>(AggregateType<T> aggregateType, string id) where T : notnull, new()
        {
            List<Event.Event> events;
            var snapshotData = await StorageAdapter.GetLatestSnapshot<T>(aggregateType.Name, id);
            if (snapshotData == null)
            {
                events = await StorageAdapter.GetStoredEvents(aggregateType.Name, id, 0);
                return aggregateType.CreateAggregate(id, events);
            }
            long nextSequenceId = GetNextSequenceId(snapshotData.SnapshotSequenceId);
            events = await StorageAdapter.GetStoredEvents(aggregateType.Name, id, nextSequenceId);
            return aggregateType.CreateAggregate(id, snapshotData.Snapshot, snapshotData.SnapshotSequenceId, events);
        }

        public async Task Store<T>(Aggregate<T> aggregate) where T : notnull, new()
        {
            if (aggregate.PendingEvents.Count == 0)
            {
                await Task.FromResult(true);
                return;
            }
            await StorageAdapter.Store<T>(aggregate, false);
            aggregate.ClearPendingEvents();
        }
    }
}