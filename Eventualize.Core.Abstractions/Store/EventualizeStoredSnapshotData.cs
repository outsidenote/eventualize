using System.Text.Json;

namespace Eventualize.Core;

public record EventualizeStoredSnapshotData<T>(T Snapshot, long SnapshotOffset) where T : notnull, new()
{
    [Obsolete("Only for Dapper")]
    public EventualizeStoredSnapshotData(string Snapshot, long SnapshotOffset) :
        this(JsonSerializer.Deserialize<T>(Snapshot) ?? new(), SnapshotOffset)
    {
    }

    public static EventualizeStoredSnapshotData<T> Create() => new EventualizeStoredSnapshotData<T>();

    public static EventualizeStoredSnapshotData<T> Create(T state) => new EventualizeStoredSnapshotData<T>(state);

    private EventualizeStoredSnapshotData()
        : this(new T(), -1) { }

    private EventualizeStoredSnapshotData(T state)
        : this(state, -1) { }
};