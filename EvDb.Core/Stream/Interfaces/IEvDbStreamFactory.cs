namespace EvDb.Core;


// TODO: [bnaya 2023-01-23] Encapsulate IEvDbStreamConfig in a property
public interface IEvDbStreamFactory<T> : IEvDbStreamConfig
    where T : IEvDbStreamStore, IEvDbEventAdder
{
    T Create(string streamId);

    Task<T> GetAsync(
        string streamId,
        CancellationToken cancellationToken = default);
}