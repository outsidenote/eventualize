using System.Text.Json;

namespace Eventualize.Core;

public record EventualizeStoredSnapshotData<T>(T Snapshot, long SnapshotSequenceId) where T : notnull, new()
{
    [Obsolete("Only for Dapper")]
    public EventualizeStoredSnapshotData(string Snapshot, long SnapshotSequenceId) :
        this(JsonSerializer.Deserialize<T>(Snapshot) ?? new(), SnapshotSequenceId)
    {
    }

    public EventualizeStoredSnapshotData()
        : this(new T(), -1) { }

    public EventualizeStoredSnapshotData(T state)
        : this(state, -1) { }
};