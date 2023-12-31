namespace EvDb.Core;

public interface IFoldingFunction<StateType>
{
    public StateType Fold(StateType oldState, IEvDbEvent serializedEvent);

}