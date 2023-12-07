namespace Core.Repository
{
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