namespace EvDb.Core.Builder;

public interface IEvDbBuilderWithEventTypesWithEntityId<TEventTypes> :
    IEvDbBuilderEntityId<IEvDbBuilderWithEventTypes<TEventTypes>>
    where TEventTypes : IEvDbEventTypes
{
    IEvDbBuilderBuildWithEntityId<TState, TEventTypes> AddAggregateType<TState>(IEvDbAggregateType<TState, TEventTypes> aggregateType);
}
