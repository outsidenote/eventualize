using Eventualize.Core.Abstractions;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;

// TODO [bnaya 2023-12-13] consider to encapsulate snapshot object with Snapshot<T> which is a wrapper of T that holds T and snapshotOffset 

namespace Eventualize.Core;

[DebuggerDisplay("LastStoredOffset: {LastStoredOffset}, State: {State}")]
public abstract class EventualizeAggregate
{
    #region Ctor

    internal EventualizeAggregate(
        string aggregateType,
        EventualizeStreamUri streamUri,
        int minEventsBetweenSnapshots,
        long lastStoredOffset)
    {
        AggregateType = aggregateType;
        StreamUri = streamUri;
        MinEventsBetweenSnapshots = minEventsBetweenSnapshots;
        LastStoredOffset = lastStoredOffset;
        SnapshotUri = new(streamUri, aggregateType);
    }

    #endregion // Ctor

    #region Members
    // [bnaya 2023-12-11] Consider what is the right data type (thread safe)
    protected internal ConcurrentQueue<IEventualizeEvent> _pendingEvents = new ConcurrentQueue<IEventualizeEvent>();
    public IImmutableList<IEventualizeEvent> PendingEvents => _pendingEvents.ToImmutableArray();

    public long LastStoredOffset { get; protected set; } = -1;
    // TODO: [bnaya 2023-12-11] Use Min Duration or Count between snapshots
    public int MinEventsBetweenSnapshots { get; init; } = 0;

    public EventualizeStreamUri StreamUri { get; init; }
    public readonly string AggregateType;

    public readonly EventualizeSnapshotUri SnapshotUri;

    #endregion // Members
}

[DebuggerDisplay("LastStoredOffset: {LastStoredOffset}, State: {State}")]
public class EventualizeAggregate<TState> : EventualizeAggregate where TState : notnull, new()
{
    #region Ctor

    internal EventualizeAggregate(
        string aggregateType,
        EventualizeStreamUri streamUri,
        EventualizeFoldingLogic<TState> foldingLogic,
        int minEventsBetweenSnapshots,
        TState state,
        long lastStoredOffset)
        : base(aggregateType, streamUri, minEventsBetweenSnapshots, lastStoredOffset)
    {
        State = state;
        FoldingLogic = foldingLogic;
    }

    internal EventualizeAggregate(string aggregateType,
                                  EventualizeStreamUri streamUri,
                                  EventualizeFoldingLogic<TState> foldingLogic,
                                  int minEventsBetweenSnapshots)
        : this(aggregateType, streamUri, foldingLogic, minEventsBetweenSnapshots, new TState(), -1) { }

    #endregion // Ctor

    #region Members

    public readonly EventualizeFoldingLogic<TState> FoldingLogic;
    public TState State { get; private set; }

    #endregion // Members

    public void AddPendingEvent(IEventualizeEvent someEvent)
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

