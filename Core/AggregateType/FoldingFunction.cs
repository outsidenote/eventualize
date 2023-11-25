using Core.Event;
namespace Core.AggregateType;
public delegate object FoldingFunction(object oldState, Event.Event SerializedEvent);