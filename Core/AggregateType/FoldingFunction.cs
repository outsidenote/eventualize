using System.Data;
using Core.Event;
namespace Core.AggregateType;
public delegate object FoldingFunction(object oldState, Event.Event SerializedEvent);

public delegate StateType FoldingFunction<StateType>(StateType oldState, Event.Event SerializedEvent);

public interface IFoldingFunction<StateType>
{
    public StateType Fold(StateType oldState, Event.Event serializedEvent);

}