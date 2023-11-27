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
            var snapshotData = await StorageAdapter.GetLatestSnapshot<T>(nameof(aggregateType), id);
            if (snapshotData == null)
            {
                events = await StorageAdapter.GetStoredEvents(0);
                return aggregateType.CreateAggregate(id, events);
            }
            long nextSequenceId = GetNextSequenceId(snapshotData.SnapshotSequenceId);
            events = await StorageAdapter.GetStoredEvents(nextSequenceId);
            return aggregateType.CreateAggregate(id, snapshotData.Snapshot, events);
        }
    }
}