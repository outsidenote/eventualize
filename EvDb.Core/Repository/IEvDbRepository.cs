
using System.Text.Json;

namespace EvDb.Core;


public interface IEvDbRepository
{
    Task<T> GetAsync<T>(
        IEvDbFactory<T> factory,
        string streamId,
        CancellationToken cancellation = default)
            where T : IEvDbStreamStore, IEvDbEventAdder;

    Task SaveAsync(IEvDbStreamStore aggregate, JsonSerializerOptions? options = null, CancellationToken cancellation = default);
}