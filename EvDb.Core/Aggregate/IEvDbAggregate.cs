using System.Collections.Immutable;

namespace EvDb.Core;

public interface IEvDbAggregate
{
    long LastStoredOffset { get; }
    int MinEventsBetweenSnapshots { get; init; }
    IImmutableList<IEvDbEvent> PendingEvents { get; }
    EvDbStreamId StreamId { get; init; }
}

public interface IEvDbAggregate<TState>: IEvDbAggregate
    where TState : notnull, new()
{
    TState State { get; }
}

public interface IEvDbAggregate<TState, TEvents>: IEvDbAggregate
    //where TState : notnull, new()
{
    TState State { get; }

    TEvents Events { get; }
}