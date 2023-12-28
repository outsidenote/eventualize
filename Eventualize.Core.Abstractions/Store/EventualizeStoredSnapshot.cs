using Generator.Equals;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Eventualize.Core;

public class EventualizeStoredSnapshot
{
    #region Create

    public static EventualizeStoredSnapshot<T> Create<T>() where T : notnull, new() => new EventualizeStoredSnapshot<T>(new T(), EventualizeSnapshotCursor.Empty);

    public static EventualizeStoredSnapshot<T> Create<T>(T state) where T : notnull, new() => new EventualizeStoredSnapshot<T>(state, EventualizeSnapshotCursor.Empty);

    public static EventualizeStoredSnapshot<T> Create<T>(
                                        EventualizeeSnapshotRelationalRecrod record,
                                        JsonSerializerOptions? options = null) where T : notnull, new()
    {
        T value = JsonSerializer.Deserialize<T>(record.SerializedState, options) ?? throw new NullReferenceException("deserialize");
        var cursor = record.ToCursor;
        var result = new EventualizeStoredSnapshot<T>(value, cursor);
        return result;
    }

    public static EventualizeStoredSnapshot<T> Create<T>(
                                        EventualizeeSnapshotRelationalRecrod record,
                                        JsonTypeInfo<T> jsonType) where T : notnull, new()
    {
        T value = JsonSerializer.Deserialize<T>(record.SerializedState, jsonType) ?? throw new NullReferenceException("deserialize");

        var cursor = record.ToCursor;
        var result = new EventualizeStoredSnapshot<T>(value, cursor);
        return result;
    }

    #endregion // Create
}

[Equatable]
public partial record EventualizeStoredSnapshot<T>(T State, EventualizeSnapshotCursor Cursor) where T : notnull, new()
{
};