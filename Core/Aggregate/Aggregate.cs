using Core.AggregateType;
using Core.Event;

namespace Core.Aggregate
{
    public class Aggregate<StateType> where StateType : notnull, new()
    {
        public readonly string Id;
        public readonly AggregateType<StateType> AggregateType;
        public List<Event.Event> PendingEvents { get; private set; } = new List<Event.Event>();
        public int LastStoredSequenceId { get; private set; } = 0;

        public StateType State { get; private set; }

        public Aggregate(AggregateType<StateType> aggregateType, string id)
        {
            AggregateType = aggregateType;
            Id = id;
            State = new StateType();
        }

        public Aggregate(AggregateType<StateType> aggregateType, string id, List<Event.Event> events)
        {
            AggregateType = aggregateType;
            Id = id;
            State = new StateType();
            State = AggregateType.FoldEvents(State, events);
        }

        public Aggregate(AggregateType<StateType> aggregateType, string id, StateType? snapshot, List<Event.Event>? events)
        {
            AggregateType = aggregateType;
            Id = id;
            State = snapshot ?? new StateType();
            if (events != null)
                State = AggregateType.FoldEvents(State, events);
        }

        public void AddPendingEvent(Event.Event someEvent)
        {
            EventType? eventType;
            if (!AggregateType.RegisteredEventTypes.TryGetValue(someEvent.EventType, out eventType))
                throw new KeyNotFoundException($"No registered event of type {someEvent.EventType}");
            eventType.ParseData(someEvent);
            PendingEvents.Add(someEvent);
            State = AggregateType.FoldEvent(State, someEvent);
        }
    }
}

