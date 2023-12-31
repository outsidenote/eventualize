
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace EvDb.Core;

public interface IEvDbRepository
{
    Task<EvDbAggregate<T>> GetAsync<T>(EvDbAggregateFactory<T> aggregateFactory, string streamId, CancellationToken cancellation = default) where T : notnull, new();
    Task SaveAsync<T>(EvDbAggregate<T> aggregate, JsonSerializerOptions? options = null, CancellationToken cancellation = default) where T : notnull, new();

    Task SaveAsync<T>(EvDbAggregate<T> aggregate, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellation = default) where T : notnull, new();
}