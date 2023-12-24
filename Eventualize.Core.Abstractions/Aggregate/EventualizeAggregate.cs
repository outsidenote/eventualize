﻿using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using Eventualize.Core.Abstractions.Stream;

// TODO [bnaya 2023-12-13] consider to encapsulate snapshot object with Snapshot<T> which is a wrapper of T that holds T and snapshotOffset 

namespace Eventualize.Core;

[DebuggerDisplay("LastStoredOffset: {LastStoredOffset}, State: {State}")]
public abstract class EventualizeAggregate
{
    #region Ctor

    internal EventualizeAggregate(
        EventualizeStreamAddress streamAddress,
        int minEventsBetweenSnapshots,
        long lastStoredOffset)
    {
        StreamAddress = streamAddress;
        MinEventsBetweenSnapshots = minEventsBetweenSnapshots;
        LastStoredOffset = lastStoredOffset;
    }

    #endregion // Ctor

    #region Members
    // [bnaya 2023-12-11] Consider what is the right data type (thread safe)
    protected internal ConcurrentQueue<EventualizeEvent> _pendingEvents = new ConcurrentQueue<EventualizeEvent>();
    public IImmutableList<EventualizeEvent> PendingEvents => _pendingEvents.ToImmutableArray();

    public long LastStoredOffset { get; protected set; } = -1;
    // TODO: [bnaya 2023-12-11] Use Min Duration or Count between snapshots
    public int MinEventsBetweenSnapshots { get; init; } = 0;

    public EventualizeStreamAddress StreamAddress { get; init; }

    #endregion // Members
}

[DebuggerDisplay("LastStoredOffset: {LastStoredOffset}, State: {State}")]
public class EventualizeAggregate<T> : EventualizeAggregate where T : notnull, new()
{
    #region Ctor

    internal EventualizeAggregate(string aggregateType, EventualizeStreamAddress streamAddress, Dictionary<string, EventualizeEventType> registeredEventTypes, EventualizeFoldingLogic<T> foldingLogic, int minEventsBetweenSnapshots, T state, long lastStoredOffset)
        : base(streamAddress, minEventsBetweenSnapshots, lastStoredOffset)
    {
        State = state;
        AggregateType = aggregateType;
        RegisteredEventTypes = registeredEventTypes;
        FoldingLogic = foldingLogic;
    }

    internal EventualizeAggregate(string aggregateType, EventualizeStreamAddress streamAddress, Dictionary<string, EventualizeEventType> registeredEventTypes, EventualizeFoldingLogic<T> foldingLogic, int minEventsBetweenSnapshots)
        : this(aggregateType, streamAddress, registeredEventTypes, foldingLogic, minEventsBetweenSnapshots, new T(), -1) { }


    #endregion // Ctor

    #region Members

    public readonly Dictionary<string, EventualizeEventType> RegisteredEventTypes;
    public readonly EventualizeFoldingLogic<T> FoldingLogic;
    public readonly string AggregateType;
    public T State { get; private set; }

    #endregion // Members

    public void AddPendingEvent(EventualizeEvent someEvent)
    {
        EventualizeEventType? eventType;
        if (!RegisteredEventTypes.TryGetValue(someEvent.EventType, out eventType))
            throw new KeyNotFoundException($"No registered event of type {someEvent.EventType}");

        // TODO: [bnaya 2023-12-18] @Roma what is it for?
        eventType.ParseData(someEvent);

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

