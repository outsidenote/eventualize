namespace EvDb.Core;


// TODO: [bnaya 2023-01-23] Encapsulate IEvDbStreamConfig in a property
/// <summary>
/// EvDB Stream factory
/// </summary>
/// <typeparam name="T"></typeparam>
/// <seealso cref="EvDb.Core.IEvDbStreamConfig" />
public interface IEvDbStreamFactory<T> : IEvDbStreamConfig
    where T : IEvDbStreamStore, IEvDbEventTypes
{
    T Create(string streamId);

    Task<T> GetAsync(
        string streamId,
        CancellationToken cancellationToken = default);
}
