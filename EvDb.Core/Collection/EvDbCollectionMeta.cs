using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;

// TODO [bnaya 2023-12-13] consider to encapsulate snapshot object with Snapshot<T> which is a wrapper of T that holds T and snapshotOffset 

namespace EvDb.Core;


[DebuggerDisplay("LastStoredOffset: {LastStoredOffset}, State: {State}")]
public abstract class EvDbCollectionMeta : IEvDbCollectionMeta
{
    // [bnaya 2023-12-11] Consider what is the right data type (thread safe)
    protected internal IImmutableList<IEvDbEvent> _pendingEvents = ImmutableList<IEvDbEvent>.Empty;

    protected static readonly SemaphoreSlim _dirtyLock = new SemaphoreSlim(1);

    #region Ctor

    internal EvDbCollectionMeta(
        string kind,
        EvDbStreamAddress streamId,
        int minEventsBetweenSnapshots,
        long lastStoredOffset,
        JsonSerializerOptions? options)
    {
        Kind = kind;
        StreamId = streamId;
        MinEventsBetweenSnapshots = minEventsBetweenSnapshots;
        LastStoredOffset = lastStoredOffset;
        Options = options;
        SnapshotId = new(streamId, kind);
    }

    #endregion // Ctor

    // TODO: [bnaya 2023-12-11] Use Min Duration or Count between snapshots

    #region MinEventsBetweenSnapshots

    public int MinEventsBetweenSnapshots { get; init; } = 0;

    #endregion // MinEventsBetweenSnapshots

    #region StreamId

    public EvDbStreamAddress StreamId { get; init; }

    #endregion // StreamId

    public string Kind { get; }

    public int EventsCount => _pendingEvents.Count;

    #region LastStoredOffset

    public long LastStoredOffset { get; protected set; } = -1;

    #endregion // LastStoredOffset

    #region SnapshotId

    public EvDbSnapshotId SnapshotId { get; }

    #endregion // SnapshotId

    #region IsEmpty

    bool IEvDbCollectionMeta.IsEmpty => _pendingEvents.Count == 0;

    #endregion // IsEmpty

    public JsonSerializerOptions? Options { get; }

    IEnumerable<IEvDbEvent> IEvDbCollectionMeta.Events => _pendingEvents;
}

