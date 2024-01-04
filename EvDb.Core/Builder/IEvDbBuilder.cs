using EvDb.Core.Builder;

namespace EvDb.Core;

public interface IEvDbBuilder
{

    IEvDbBuilderWithEventTypesWithEntityId<TEventTypes> AddPartition<TEventTypes>(string domain, string partition)
            where TEventTypes : IEvDbEventTypes;
}
