namespace Eventualize.Core
{
    public class EventualizeStoredSnapshotData<T>
    {
        public readonly T Snapshot;
        public readonly long SnapshotSequenceId;

        public EventualizeStoredSnapshotData(T snapshot, long snapshotSequenceId)
        {
            Snapshot = snapshot;
            SnapshotSequenceId = snapshotSequenceId;
        }

    }
}