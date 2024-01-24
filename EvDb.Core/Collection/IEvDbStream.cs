namespace EvDb.Core;

public interface IEvDbStream : IEvDbCollectionMeta
{
    /// <summary>
    /// Saves pending events into the injected storage.
    /// </summary>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    Task SaveAsync(CancellationToken cancellation = default);

    // TODO: [bnaya 2024-01-09] selective clear is needed
    void ClearLocalEvents();

    //Task GetAsync<TState>(string streamId, CancellationToken cancellation = default);
}
