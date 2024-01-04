using EvDb.Core.Builder;
using EvDb.Core;

public class EvDbBuilder : IEvDbBuilder
{
    protected readonly EvDbPartition? _streamType;

    protected EvDbBuilder(EvDbPartition? streamType = null)
    {
        _streamType = streamType;
    }

    public static IEvDbBuilder Default { get; } = new EvDbBuilder();

    IEvDbBuilderWithEventTypesWithEntityId<TEventTypes> IEvDbBuilder.AddPartition<TEventTypes>(string domain, string partition)
    {
        EvDbPartition streamType = new(domain, partition);
        return new EvDbBuilder<TEventTypes>(streamType);
    }
}

internal class EvDbBuilder<TEventTypes> :
                            EvDbBuilder,
                            IEvDbBuilderWithEventTypes<TEventTypes>,
                            IEvDbBuilderWithEventTypesWithEntityId<TEventTypes>
    where TEventTypes : IEvDbEventTypes
{
    protected readonly EvDbPartition _streamType;
    protected readonly string _streamId = string.Empty;

    public EvDbBuilder(EvDbPartition streamType) : base(streamType)
    {
        _streamType = streamType;
    }

    protected EvDbBuilder(EvDbPartition streamType, string streamId) : base(streamType)
    {
        _streamType = streamType;
        _streamId = streamId;
    }

    private EvDbBuilder<TEventTypes, TState> AddAggregateType<TState>(IEvDbAggregateType<TState, TEventTypes> aggregateType)
    {
        EvDbBuilder<TEventTypes, TState> result = new EvDbBuilder<TEventTypes, TState>(
            _streamType, aggregateType, _streamId);
        return result;
    }

    IEvDbBuilderBuildWithEntityId<TState, TEventTypes> IEvDbBuilderWithEventTypesWithEntityId<TEventTypes>.AddAggregateType<TState>(IEvDbAggregateType<TState, TEventTypes> aggregateType)
    {
        return AddAggregateType(aggregateType);
    }

    IEvDbBuilderBuild<TState, TEventTypes> IEvDbBuilderWithEventTypes<TEventTypes>.AddAggregateType<TState>(IEvDbAggregateType<TState, TEventTypes> aggregateType)
    {
        return AddAggregateType(aggregateType);
    }

    IEvDbBuilderWithEventTypes<TEventTypes> IEvDbBuilderEntityId<IEvDbBuilderWithEventTypes<TEventTypes>>.AddStreamId(string id)
    {
        return new EvDbBuilder<TEventTypes>(_streamType, id);
    }
}

internal class EvDbBuilder<TEventTypes, TState> :
                            EvDbBuilder<TEventTypes>,
                            IEvDbBuilderBuildWithEntityId<TState, TEventTypes>
    where TEventTypes : IEvDbEventTypes
{
    private readonly IEvDbAggregateType<TState, TEventTypes> _aggregateType;

    public EvDbBuilder(
                EvDbPartition streamType,
                IEvDbAggregateType<TState, TEventTypes> aggregateType,
                string streamId = "") : base(streamType, streamId)
    {
        _aggregateType = aggregateType;
    }

    IEvDbBuilderBuild<TState, TEventTypes> IEvDbBuilderEntityId<IEvDbBuilderBuild<TState, TEventTypes>>.AddStreamId(string id)
    {
        return new EvDbBuilder<TEventTypes, TState>(_streamType, _aggregateType, id);
    }

    IEvDbAggregate<TState, TEventTypes> IEvDbBuilderBuild<TState, TEventTypes>.Build()
    {
        // var aggregation = EvDbAggregateFactory.Create()
        throw new NotImplementedException();
    }
}
