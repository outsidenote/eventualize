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
        public Task<GetLatestSnapshotData<T>?> GetLatestSnapshot<T>(string aggregateTypeName, string id) where T : notnull, new();
        public Task<List<Event.Event>> GetStoredEvents(long startSequenceId);
        public Task<List<Core.Event.Event>?> Store<T>(Aggregate<T> aggregate, bool storeSnapshot) where T : notnull, new();
    }

    public class GetLatestSnapshotData<T>
    {
        public readonly T? Snapshot;
        public readonly long? SnapshotSequenceId;

        public GetLatestSnapshotData(T? snapshot, long? snapshotSequenceId)
        {
            Snapshot = snapshot;
            SnapshotSequenceId = snapshotSequenceId;
        }

    }
}