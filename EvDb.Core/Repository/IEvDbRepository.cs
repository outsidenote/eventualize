
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace EvDb.Core;

// TODO: [bnaya 2024-01-08] When Aggregate gets IEvDbStorageAdapter it can create repository, therefore no need to get or return the IEvDbAggregate<TState>
public interface IEvDbRepository
{
    //Task<IEvDbAggregate<TState>> GetAsync<TState>(IEvDbAggregate<TState> aggregate, CancellationToken cancellation = default);
    Task<T> GetAsync<T, TState>(
        IEvDbAggregateFactory<T, TState> factory,
        string streamId, 
        CancellationToken cancellation = default)
            where T : IEvDbAggregate<TState>, IEvDbEventTypes;

    Task SaveAsync<TState>(IEvDbAggregate<TState> aggregate, JsonSerializerOptions? options = null, CancellationToken cancellation = default);
}