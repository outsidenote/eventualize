using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;

// TODO [bnaya 2023-12-13] consider to encapsulate snapshot object with Snapshot<T> which is a wrapper of T that holds T and snapshotOffset 

namespace EvDb.Core;

[DebuggerDisplay("LastStoredOffset: {LastStoredOffset}, State: {State}")]
public abstract class EvDbAggregate
{
    #region Ctor

    internal EvDbAggregate(
        string aggregateType,
        EvDbStreamId streamId,
        int minEventsBetweenSnapshots,
        long lastStoredOffset)
    {
        AggregateType = aggregateType;
        StreamId = streamId;
        MinEventsBetweenSnapshots = minEventsBetweenSnapshots;
        LastStoredOffset = lastStoredOffset;
        SnapshotId = new(streamId, aggregateType);
    }

    #endregion // Ctor

    #region Members
    // [bnaya 2023-12-11] Consider what is the right data type (thread safe)
    protected internal ConcurrentQueue<IEvDbEvent> _pendingEvents = new ConcurrentQueue<IEvDbEvent>();
    public IImmutableList<IEvDbEvent> PendingEvents => _pendingEvents.ToImmutableArray();

    public long LastStoredOffset { get; protected set; } = -1;
    // TODO: [bnaya 2023-12-11] Use Min Duration or Count between snapshots
    public int MinEventsBetweenSnapshots { get; init; } = 0;

    public EvDbStreamId StreamId { get; init; }
    public readonly string AggregateType;

    public readonly EvDbSnapshotId SnapshotId;

    #endregion // Members
}

[DebuggerDisplay("LastStoredOffset: {LastStoredOffset}, State: {State}")]
public class EvDbAggregate<TState> : EvDbAggregate where TState : notnull, new()
{
    #region Ctor

    internal EvDbAggregate(
        string aggregateType,
        EvDbStreamId streamId,
        EvDbFoldingLogic<TState> foldingLogic,
        int minEventsBetweenSnapshots,
        TState state,
        long lastStoredOffset)
        : base(aggregateType, streamId, minEventsBetweenSnapshots, lastStoredOffset)
    {
        State = state;
        FoldingLogic = foldingLogic;
    }

    internal EvDbAggregate(string aggregateType,
                                  EvDbStreamId streamId,
                                  EvDbFoldingLogic<TState> foldingLogic,
                                  int minEventsBetweenSnapshots)
        : this(aggregateType, streamId, foldingLogic, minEventsBetweenSnapshots, new TState(), -1) { }

    #endregion // Ctor

    #region Members

    public readonly EvDbFoldingLogic<TState> FoldingLogic;
    public TState State { get; private set; }

    #endregion // Members

    public void AddPendingEvent(IEvDbEvent someEvent)
    {
        _pendingEvents.Enqueue(someEvent);
        // TODO: [bnaya 2023-12-19] thread safe
        State = FoldingLogic.FoldEvent(State, someEvent);
    }

    public void ClearPendingEvents()
    {
        LastStoredOffset += PendingEvents.Count;
        _pendingEvents.Clear();
    }
}

