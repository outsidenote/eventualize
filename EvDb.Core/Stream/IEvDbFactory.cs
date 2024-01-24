namespace EvDb.Core;


// TODO: [bnaya 2023-01-23] Encapsulate IEvDbStreamConfig in a property
public interface IEvDbFactory<T> : IEvDbStreamConfig  
    where T : IEvDbStreamStore, IEvDbEventAdder
{

    T Create(string streamId,
             long lastStoredOffset = -1);
    // T Create(EvDbStoredSnapshot<TState> snapshot);

    Task<T> GetAsync(
        string streamId, long lastStoredOffset = -1, CancellationToken cancellationToken = default);
}