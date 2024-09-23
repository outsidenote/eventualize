namespace EvDb.Core;


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
