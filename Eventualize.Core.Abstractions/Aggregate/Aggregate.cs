using Eventualize.Core.AggregateType;

namespace Eventualize.Core.Aggregate;

public class Aggregate<StateType> where StateType : notnull, new()
{
    public string Id { get; private set; }
    public AggregateType<StateType> AggregateType { get; private set; }
    // [bnaya 2023-12-11] Consider what is the right data type (thread safe)
    public List<EventEntity> PendingEvents { get; private set; } = new List<EventEntity>();
    public long LastStoredSequenceId { get; private set; } = -1;
    // TODO: [bnaya 2023-12-11] Use Min Duration or Count between snapshots
    public int MinEventsBetweenSnapshots { get; private set; } = 0;

    public StateType State { get; private set; }

    public Aggregate(
        AggregateType<StateType> aggregateType,
        string id,
        int minEventsBetweenSnapshots)
    {
        AggregateType = aggregateType;
        Id = id;
        MinEventsBetweenSnapshots = minEventsBetweenSnapshots;
        State = new StateType();
    }

    public Aggregate(
        AggregateType<StateType> aggregateType,
        string id,
        int minEventsBetweenSnapshots,
        ICollection<EventEntity> events)
            : this(aggregateType, id, minEventsBetweenSnapshots)
    {
        State = AggregateType.FoldEvents(State, events);
        LastStoredSequenceId = events.Count - 1;
    }

    public Aggregate(
        AggregateType<StateType> aggregateType,
        string id,
        int minEventsBetweenSnapshots,
        StateType snapshot,
        long snapshotSequenceId,
        ICollection<EventEntity> events)
            : this(aggregateType, id, minEventsBetweenSnapshots)
    {
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

