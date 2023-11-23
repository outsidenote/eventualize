using Core.Event;
namespace Core.AggregateType;
public delegate StateType FoldingFunction<StateType>(StateType oldState, Event<string> SerializedEvent);