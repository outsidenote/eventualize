using System.Collections.Immutable;

namespace EvDb.Core;

public interface IEvDbStreamStore
{
    /// <summary>
    /// Saves pending events into the injected storage.
    /// </summary>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    Task SaveAsync(CancellationToken cancellation = default);
}
