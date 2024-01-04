namespace EvDb.Core;

public interface IEvDbAggregateType<TState, TEventTypes>
{
    /// <summary>
    /// Represents the seed state of the aggregation (before folding).
    /// </summary>
    TState Default { get; }

    /// <summary>
    /// Gets the name.
    /// </summary>
    string Name { get; }
}
