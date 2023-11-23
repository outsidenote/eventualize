using System.Text.Json;
using Core.Aggregate;
using Core.AggregateType;
using Core.Event;

namespace Core
{
    public class Aggregate<StateType>
    {
        public readonly AggregateType<StateType> AggregateType;
        public PendingEvent[] PendingEvents { get; private set; } = Array.Empty<PendingEvent>();
        public int LastStoredSequenceId { get; private set; } = 0;

        public Aggregate(AggregateType<StateType> aggregateType)
        {
            AggregateType = aggregateType;
        }

        public void AddPendingEvent<EventDataType>(Event<EventDataType> someEvent)
        {
            PendingEvent pendingEvent = PendingEvent.create<EventDataType>(someEvent);
            PendingEvents.Append(pendingEvent);
        }
    }
}

