namespace EvDb.Core.Builder;

public interface IEvDbBuilderBuild<TState, TEventTypes>
    // where TState: notnull, new()
    where TEventTypes : IEvDbEventTypes
{
    IEvDbAggregate<TState, TEventTypes> Build();
}
