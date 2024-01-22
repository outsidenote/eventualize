namespace EvDb.Core;

[Obsolete("Deprecated")]
public interface IFoldingFunction<StateType>
{
    public StateType Fold(StateType oldState, IEvDbEvent serializedEvent);

}