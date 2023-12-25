namespace Eventualize.Core;

public interface IFoldingFunction<StateType>
{
    public StateType Fold(StateType oldState, IEventualizeEvent serializedEvent);

}