namespace EvDb.Core;


/// <summary>
/// EvDB Stream factory
/// </summary>
/// <typeparam name="T"></typeparam>
/// <seealso cref="EvDb.Core.IEvDbStreamConfig" />
public interface IEvDbStreamFactory<T> : IEvDbStreamConfig
    where T : IEvDbStreamStore, IEvDbEventTypes
{
    /// <summary>
    /// Create a stream without fetching the stream data from the storage
    /// </summary>
    /// <typeparam name="TId"></typeparam>
    /// <param name="streamId">
    /// The stream identifier.
    /// It's the dynamic part of the stream address along with the static part of `address` 
    /// </param>
    /// <returns></returns>
    T Create<TId>(in TId streamId)
        where TId : notnull;

    /// <summary>
    /// Get a stream from the storage
    /// </summary>
    /// <typeparam name="TId"></typeparam>
    /// <param name="streamId">
    /// The stream identifier.
    /// It's the dynamic part of the stream address along with the static part of `address` 
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<T> GetAsync<TId>(
        TId streamId,
        CancellationToken cancellationToken = default)
        where TId : notnull;
}
