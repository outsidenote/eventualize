
using System.Text.Json;

namespace EvDb.Core;

// TODO: [bnaya 2024-01-08] When Aggregate gets IEvDbStorageAdapter it can create repository, therefore no need to get or return the IEvDbAggregateDeprecated<TState>
[Obsolete("Deprecated")]
public interface IEvDbRepository
{
    //Task<IEvDbAggregateDeprecated<TState>> GetAsync<TState>(IEvDbAggregateDeprecated<TState> aggregate, CancellationToken cancellation = default);
    Task<T> GetAsync<T, TState>(
        IEvDbAggregateFactory<T, TState> factory,
        string streamId,
        CancellationToken cancellation = default)
            where T : IEvDbAggregateDeprecated<TState>, IEvDbEventAdder;

    Task SaveAsync<TState>(IEvDbAggregateDeprecated<TState> aggregate, JsonSerializerOptions? options = null, CancellationToken cancellation = default);
}

public interface IEvDbRepositoryV1
{
    //Task<IEvDbAggregateDeprecated<TState>> GetAsync<TState>(IEvDbAggregateDeprecated<TState> aggregate, CancellationToken cancellation = default);
    Task<T> GetAsync<T>(
        IEvDbFactory<T> factory,
        string streamId,
        CancellationToken cancellation = default)
            where T : IEvDbCollection, IEvDbEventAdder;

    Task SaveAsync(IEvDbCollection aggregate, JsonSerializerOptions? options = null, CancellationToken cancellation = default);
}