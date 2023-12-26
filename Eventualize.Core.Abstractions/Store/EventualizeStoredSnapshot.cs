using System.Text.Json;

namespace Eventualize.Core;

public record EventualizeStoredSnapshot<T>(T State, EventualizeSnapshotCursor Cursor) where T : notnull, new()
{
    [Obsolete("Only for Dapper")]
    public EventualizeStoredSnapshot(string serializedState, string domain, string streamType, string streamId, string aggregateType, long offset) :
        this(JsonSerializer.Deserialize<T>(serializedState) ?? new(), new EventualizeSnapshotCursor(domain, streamType, streamId, aggregateType, offset))
    {
    }

    private EventualizeStoredSnapshot(EventualizeeSnapshotRelationalRecrod record) :
        this(JsonSerializer.Deserialize<T>(record.SerializedState) ?? new(), record)
    {
    }

    public static EventualizeStoredSnapshot<T> Create() => new EventualizeStoredSnapshot<T>();

    public static EventualizeStoredSnapshot<T> Create(T state) => new EventualizeStoredSnapshot<T>(state);
    public static EventualizeStoredSnapshot<T> Create(EventualizeeSnapshotRelationalRecrod record) => new EventualizeStoredSnapshot<T>(record);

    private EventualizeStoredSnapshot()
        : this(new T(), new EventualizeSnapshotCursor("N/A", "N/A", "N/A", "N/A", -1)) { }

    private EventualizeStoredSnapshot(T state)
        : this(state, new EventualizeSnapshotCursor("N/A", "N/A", "N/A", "N/A", -1)) { }

};