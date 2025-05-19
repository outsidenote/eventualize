using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions;

public static class EvDbTestingStreamStoreExtensions
{
    #region ToAsync

    private static async IAsyncEnumerable<T> ToAsync<T>(this IEnumerable<T> collection)
    {
        foreach (var item in collection)
        {
            await Task.Yield();
            yield return item;
        }
    }

    #endregion //  ToAsync

    #region GetAllRecordMessagesAsync

    public static IAsyncEnumerable<EvDbMessageRecord> GetAllRecordMessagesAsync(this EvDbStreamTestingStorage storage,
                                                                                EvDbShardName shard) =>
                                                                                   storage.GetAllRecordMessages(shard)
                                                                                        .ToAsync();

    public static IAsyncEnumerable<EvDbMessageRecord> GetAllRecordMessagesAsync(this EvDbStreamTestingStorage storage,
                                                                    Func<EvDbMessage, bool>? predicate = null) =>
                                                                                   storage.GetAllRecordMessages(predicate)
                                                                                        .ToAsync();

    #endregion //  GetAllRecordMessagesAsync

    #region GetAllMessagesAsync

    public static IAsyncEnumerable<EvDbMessage> GetAllMessagesAsync(this EvDbStreamTestingStorage storage,
                                                              EvDbShardName shard) =>
                                                                                   storage.GetAllMessages(shard)
                                                                                        .ToAsync();

    public static IAsyncEnumerable<EvDbMessage> GetAllMessagesAsync(this EvDbStreamTestingStorage storage,
                                                              Func<EvDbMessage, bool>? predicate = null) =>
                                                                                   storage.GetAllMessages(predicate)
                                                                                        .ToAsync();

    #endregion //  GetAllMessagesAsync

    #region GetAllRecordMessages

    public static IEnumerable<EvDbMessageRecord> GetAllRecordMessages(this EvDbStreamTestingStorage storage,
                                                               EvDbShardName shard)
    {
        return storage.GetAllRecordMessages(m => m.ShardName == shard);
    }

    public static IEnumerable<EvDbMessageRecord> GetAllRecordMessages(this EvDbStreamTestingStorage storage,
                                                               Func<EvDbMessage, bool>? predicate = null)
    {
        foreach (var m in storage.GetAllMessages(predicate))
        {
            yield return m;
        }
    }

    #endregion //  GetAllRecordMessages

    #region GetAllMessages

    public static IEnumerable<EvDbMessage> GetAllMessages(this EvDbStreamTestingStorage storage,
                                                          EvDbShardName shard)
    {
        return storage.GetAllMessages(m => m.ShardName == shard);
    }

    public static IEnumerable<EvDbMessage> GetAllMessages(this EvDbStreamTestingStorage storage,
                                                          Func<EvDbMessage, bool>? predicate = null)
    {
        predicate = predicate ?? (_ => true);
        foreach (var kv in storage.Store)
        {
            foreach (var m in kv.Value.Messages.Where(predicate))
            {
                yield return m;
            }
        }
    }

    #endregion //  GetAllMessages

    #region GetRecordMessages

    public static IEnumerable<EvDbMessage> GetRecordMessages(this EvDbStreamTestingStorage storage,
                                                             EvDbStreamAddress address,
                                                             EvDbShardName shard)
    {
        foreach (var m in storage.GetMessages(address, shard))
        {
            yield return m;
        }
    }

    public static IEnumerable<EvDbMessage> GetRecordMessages(this EvDbStreamTestingStorage storage,
                                                             EvDbStreamAddress address, 
                                                             Func<EvDbMessage, bool>? predicate = null)
    {
        foreach (var m in storage.GetMessages(address, predicate))
        {
            yield return m;
        }
    }

    #endregion //  GetRecordMessages

    #region GetMessages

    public static IEnumerable<EvDbMessage> GetMessages(this EvDbStreamTestingStorage storage,
                                                       EvDbStreamAddress address,
                                                       EvDbShardName shard)
    {
        return storage.GetMessages(address, m => m.ShardName == shard);
    }

    public static IEnumerable<EvDbMessage> GetMessages(this EvDbStreamTestingStorage storage,
                                                       EvDbStreamAddress address,
                                                       Func<EvDbMessage, bool>? predicate = null)
    {
        predicate = predicate ?? (_ => true);
        if (storage.Store.TryGetValue(address, out EvDbTestingStreamData? storedStream))
        {
            foreach (var message in storedStream.Messages.Where(predicate))
            {
                yield return message;
            }
        }
    }

    #endregion //  GetMessages
}
