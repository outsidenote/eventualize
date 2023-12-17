namespace Eventualize.Core;
public delegate object EventualizeFoldingFunction(object oldState, EventualizeEvent SerializedEvent);

public delegate StateType FoldingFunction<StateType>(StateType oldState, EventualizeEvent SerializedEvent);

public interface IFoldingFunction<StateType>
{
    public StateType Fold(StateType oldState, EventualizeEvent serializedEvent);

}