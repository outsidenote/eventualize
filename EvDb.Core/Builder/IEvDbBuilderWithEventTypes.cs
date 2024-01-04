namespace EvDb.Core.Builder;

public interface IEvDbBuilderWithEventTypes<TEventTypes>
    where TEventTypes : IEvDbEventTypes
{
    IEvDbBuilderBuild<TState, TEventTypes> AddAggregateType<TState>(IEvDbAggregateType<TState, TEventTypes> aggregateType);
}
