using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Generator.Equals;

namespace Eventualize.Core;

[Equatable]
public partial record EventualizeStoredSnapshot<T>(T State, EventualizeSnapshotCursor Cursor) where T : notnull, new()
{
    public static EventualizeStoredSnapshot<T> Create() => new EventualizeStoredSnapshot<T>();

    public static EventualizeStoredSnapshot<T> Create(T state) => new EventualizeStoredSnapshot<T>(state);
    public static EventualizeStoredSnapshot<T> Create(
                                        EventualizeeSnapshotRelationalRecrod record,
                                        JsonSerializerOptions? options = null)
    {
        T value = JsonSerializer.Deserialize<T>(record.SerializedState, options) ?? throw new NullReferenceException("deserialize");
        var result = new EventualizeStoredSnapshot<T>(value,record.ToCursor);
        return result;
    }

    public static EventualizeStoredSnapshot<T> Create(
                                        EventualizeeSnapshotRelationalRecrod record,
                                        JsonTypeInfo<T> jsonType)
    {
        T value = JsonSerializer.Deserialize<T>(record.SerializedState, jsonType) ?? throw new NullReferenceException("deserialize");
        var result = new EventualizeStoredSnapshot<T>(value);
        return result;
    }

    // // TODO: [bnaya 2023-12-27] Consider Factory with JsonSerializationOption
    // private EventualizeStoredSnapshot(EventualizeeSnapshotRelationalRecrod record) :
    //     this(JsonSerializer.Deserialize<T>(record.SerializedState) ?? new(), record)
    // {
    // }

    private EventualizeStoredSnapshot()
        : this(new T(), EventualizeSnapshotCursor.Empty) { }

    private EventualizeStoredSnapshot(T state)
        : this(state, EventualizeSnapshotCursor.Empty) { }

};