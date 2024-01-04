namespace EvDb.Core.Builder;

public interface IEvDbBuilderBuildWithEntityId<TState, TEventTypes> :
    IEvDbBuilderBuild<TState, TEventTypes>,
    IEvDbBuilderEntityId<IEvDbBuilderBuild<TState, TEventTypes>>
    // where TState: notnull, new()
    where TEventTypes : IEvDbEventTypes
{
}
