using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Aggregate;

namespace Core.Repository
{
    public interface IStorageAdapter
    {
        public Task Init();
        public Task<StoredSnapshotData<T>?> GetLatestSnapshot<T>(string aggregateTypeName, string id) where T : notnull, new();
        public Task<List<Event.Event>> GetStoredEvents(string aggregateTypeName, string id, long startSequenceId);
        public Task<List<Event.Event>?> Store<T>(Aggregate<T> aggregate, bool storeSnapshot) where T : notnull, new();
        public Task<long> GetLastStoredSequenceId<T>(Aggregate<T> aggregate) where T : notnull, new();
    }

    public class StoredSnapshotData<T>
    {
        public readonly T Snapshot;
        public readonly long SnapshotSequenceId;

        public StoredSnapshotData(T snapshot, long snapshotSequenceId)
        {
            Snapshot = snapshot;
            SnapshotSequenceId = snapshotSequenceId;
        }

    }
}