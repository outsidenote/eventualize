
using System.Text.Json;

namespace EvDb.Core;

public interface IEvDbViewFactory
{
    string ViewName { get; }

    IEvDbViewStore CreateEmpty(EvDbStreamAddress address,
        JsonSerializerOptions? options,
        TimeProvider? timeProvider = null);

    Task<IEvDbViewStore> GetAsync(
        EvDbViewAddress address,
        JsonSerializerOptions? options,
        TimeProvider? timeProvider = null,
        CancellationToken cancellationToken = default);
}
