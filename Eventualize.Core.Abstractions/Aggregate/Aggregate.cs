using System.Diagnostics.CodeAnalysis;
using Core.AggregateType;
using Core;

namespace Core.Aggregate;

public class Aggregate<StateType> where StateType : notnull, new()
{
    public string Id { get; private set; }
    public AggregateType<StateType> AggregateType { get; private set; }
    public List<EventEntity> PendingEvents { get; private set; } = new List<EventEntity>();
    public long LastStoredSequenceId { get; private set; } = -1;
    public int MinEventsBetweenSnapshots { get; private set; } = 0;

    public StateType State { get; private set; }

    [MemberNotNull(nameof(Id), nameof(AggregateType), nameof(MinEventsBetweenSnapshots), nameof(State))]
    private void BaseInit(AggregateType<StateType> aggregateType, string id, int minEventsBetweenSnapshots)
    {
        AggregateType = aggregateType;
        Id = id;
        MinEventsBetweenSnapshots = minEventsBetweenSnapshots;
        State = new StateType();

    }

    public Aggregate(AggregateType<StateType> aggregateType, string id, int minEventsBetweenSnapshots)
    {
        BaseInit(aggregateType, id, minEventsBetweenSnapshots);
    }

    public Aggregate(AggregateType<StateType> aggregateType, string id, int minEventsBetweenSnapshots, List<EventEntity> events)
    {
        BaseInit(aggregateType, id, minEventsBetweenSnapshots);
        State = AggregateType.FoldEvents(State, events);
        LastStoredSequenceId = events.Count - 1;
    }

    public Aggregate(AggregateType<StateType> aggregateType, string id, int minEventsBetweenSnapshots, StateType snapshot, long snapshotSequenceId, List<EventEntity> events)
    {
        BaseInit(aggregateType, id, minEventsBetweenSnapshots);
        State = snapshot;
        State = AggregateType.FoldEvents(State, events);
        LastStoredSequenceId = snapshotSequenceId + events.Count;
    }

    public void AddPendingEvent(EventEntity someEvent)
    {
        EventType? eventType;
        if (!AggregateType.RegisteredEventTypes.TryGetValue(someEvent.EventType, out eventType))
            throw new KeyNotFoundException($"No registered event of type {someEvent.EventType}");
        eventType.ParseData(someEvent);
        PendingEvents.Add(someEvent);
        State = AggregateType.FoldEvent(State, someEvent);
    }

    public void ClearPendingEvents()
    {
        LastStoredSequenceId += PendingEvents.Count;
        PendingEvents = new();
    }
}

