﻿using Generator.Equals;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace EvDb.Core;

public class EvDbStoredSnapshot
{
    #region Create

    public static EvDbStoredSnapshot<T> Create<T>() where T : notnull, new() => new EvDbStoredSnapshot<T>(new T(), EvDbSnapshotCursor.Empty);

    public static EvDbStoredSnapshot<T> Create<T>(T state) where T : notnull, new() => new EvDbStoredSnapshot<T>(state, EvDbSnapshotCursor.Empty);

    public static EvDbStoredSnapshot<T> Create<T>(
                                        EvDbeSnapshotRelationalRecrod record,
                                        JsonSerializerOptions? options = null) where T : notnull, new()
    {
        T value = JsonSerializer.Deserialize<T>(record.SerializedState, options) ?? throw new NullReferenceException("deserialize");
        var cursor = record.ToCursor;
        var result = new EvDbStoredSnapshot<T>(value, cursor);
        return result;
    }

    public static EvDbStoredSnapshot<T> Create<T>(
                                        EvDbeSnapshotRelationalRecrod record,
                                        JsonTypeInfo<T> jsonType) where T : notnull, new()
    {
        T value = JsonSerializer.Deserialize<T>(record.SerializedState, jsonType) ?? throw new NullReferenceException("deserialize");

        var cursor = record.ToCursor;
        var result = new EvDbStoredSnapshot<T>(value, cursor);
        return result;
    }

    #endregion // Create
}

[Equatable]
public partial record EvDbStoredSnapshot<T>(T State, EvDbSnapshotCursor Cursor) where T : notnull, new()
{
};