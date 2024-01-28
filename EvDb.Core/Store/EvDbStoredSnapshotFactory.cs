using Generator.Equals;
using System.Diagnostics;
using System.Text.Json;

namespace EvDb.Core;

[Obsolete("Deprecated", true)]
public static class EvDbStoredSnapshotFactory
{
    #region Create

    public static EvDbStoredSnapshotDeprecated<T> Create<T>(T state) =>
        new EvDbStoredSnapshotDeprecated<T>(state, EvDbSnapshotCursor.Empty);

    public static EvDbStoredSnapshotDeprecated<T> Create<T>(
                                        EvDbeSnapshotRelationalRecrod record,
                                        JsonSerializerOptions? options = null)
    {
        T value = JsonSerializer.Deserialize<T>(record.SerializedState, options) ??
            throw new NullReferenceException("deserialize");
        var cursor = record.ToCursor;
        var result = new EvDbStoredSnapshotDeprecated<T>(value, cursor);
        return result;
    }

    #endregion // Create
}

[Obsolete("Deprecated", true)]
[Equatable]
[DebuggerDisplay("{State}, Offset:{Cursor.Offset}")]
public partial record EvDbStoredSnapshotDeprecated<T>(T State, EvDbSnapshotCursor Cursor)
{
};

[Equatable]
[DebuggerDisplay("{State}, Offset:{Cursor.Offset}")]
public partial record EvDbStoredSnapshot(
            long Offset,
            string State)
{
    public static readonly EvDbStoredSnapshot Empty = new EvDbStoredSnapshot(-1, string.Empty);
}