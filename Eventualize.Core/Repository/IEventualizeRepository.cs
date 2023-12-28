
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Eventualize.Core;

public interface IEventualizeRepository
{
    Task<EventualizeAggregate<T>> GetAsync<T>(EventualizeAggregateFactory<T> aggregateFactory, string streamId, CancellationToken cancellation = default) where T : notnull, new();
    Task SaveAsync<T>(EventualizeAggregate<T> aggregate, JsonSerializerOptions? options = null, CancellationToken cancellation = default) where T : notnull, new();

    Task SaveAsync<T>(EventualizeAggregate<T> aggregate, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellation = default) where T : notnull, new();
}