// Ignore Spelling: Occ

using Dapper;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static EvDb.Core.Adapters.StoreTelemetry;

namespace EvDb.Core.Adapters;

/// <summary>
/// Store adapter for rational database
/// </summary>
/// <seealso cref="EvDb.Core.IEvDbStorageAdapter" />
public abstract class EvDbRelationalStorageAdapter :
    IEvDbStorageStreamAdapter,
    IEvDbStorageSnapshotAdapter
{
    private readonly static ActivitySource _trace = StoreTelemetry.Trace;
    protected readonly ILogger _logger;
    private readonly IEvDbConnectionFactory _factory;
    private readonly IImmutableList<IEvDbOutboxTransformer> _transformers;
    private const int RETRY_COUNT = 4;
    private const int RETRY_COUNT_DELAY_MILLI = 2;

    #region Ctor

    protected EvDbRelationalStorageAdapter(ILogger logger,
        IEvDbConnectionFactory factory, IEnumerable<IEvDbOutboxTransformer> transformers)
    {
        _logger = logger;
        _factory = factory;
        _transformers = transformers.ToImmutableList();
    }

    async Task<DbConnection> InitAsync()
    {
        DbConnection connection = _factory.CreateConnection();
        await connection.OpenAsync();
        return connection;
    }

    #endregion // Ctor

    #region ExecuteSafe

    private async Task<T> ExecuteSafe<T>(Func<DbConnection, Task<T>> handler)
    {
        int delay = RETRY_COUNT_DELAY_MILLI;
        Exception? exception = null;
        for (int i = 0; i < RETRY_COUNT; i++)
        {
            DbConnection? conn = null;
            try
            {
                conn = await InitAsync();
                var result = await handler(conn!);
                return result;
            }
            #region Exception Handling

            catch (InvalidOperationException ex) when (ex.Message == "Invalid operation. The connection is closed.")
            {
                exception = ex;
                await Task.Delay(delay);
                delay *= 2;
            }

            #endregion //  Exception Handling
            finally
            {
                await (conn?.DisposeAsync() ?? ValueTask.CompletedTask);
            }
        }
        throw new DataException("Failed to execute", exception);
    }

    #endregion //  ExecuteSafe

    #region DbType

    /// <summary>
    /// Gets the type of the database.
    /// </summary>
    protected abstract string DatabaseType { get; }

    #endregion //  DatabaseType

    #region StreamQueries

    /// <summary>
    /// Gets the stream's queries.
    /// </summary>
    protected abstract EvDbStreamAdapterQueryTemplates StreamQueries { get; }

    #endregion //  StreamQueries

    #region SnapshotQueries

    /// <summary>
    /// Gets the snapshot's queries.
    /// </summary>
    protected abstract EvDbSnapshotAdapterQueryTemplates SnapshotQueries { get; }
    #endregion //  SnapshotQueries

    #region OnStoreStreamEventsAsync

    /// <summary>
    /// Storing events
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="query"></param>
    /// <param name="records"></param>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task<int> OnStoreStreamEventsAsync(
        DbConnection connection,
        string query,
        EvDbEventRecord[] records,
        DbTransaction transaction,
        CancellationToken cancellationToken)
    {
        int affctedEvents = await connection.ExecuteAsync(query, records, transaction);
        return affctedEvents;
    }

    #endregion //  OnStoreStreamEventsAsync

    #region OnStoreOutboxMessagesAsync

    /// <summary>
    /// Store outbox messages
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="shardName"></param>
    /// <param name="query"></param>
    /// <param name="records"></param>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task<int> OnStoreOutboxMessagesAsync(
        DbConnection connection,
        EvDbShardName shardName,
        string query,
        EvDbMessageRecord[] records,
        DbTransaction transaction,
        CancellationToken cancellationToken)
    {
        int affctedMessages = await connection.ExecuteAsync(query, records, transaction);
        return affctedMessages;
    }

    #endregion //  OnStoreOutboxMessagesAsync

    #region OnStoreSnapshotAsync

    /// <summary>
    /// Store a snapshot.
    /// </summary>
    /// <param name="connecion">The connection.</param>
    /// <param name="query">The save snapshot query.</param>
    /// <param name="snapshot">The snapshot data.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task<int> OnStoreSnapshotAsync(DbConnection connecion,
                                              string query,
                                              EvDbStoredSnapshotData snapshot,
                                              CancellationToken cancellationToken)
    {
        return connecion.ExecuteAsync(query, snapshot);
    }

    #endregion //  OnStoreSnapshotAsync

    #region StoreOutboxAsync

    private async Task<IImmutableDictionary<EvDbShardName, int>> StoreOutboxAsync(
        IImmutableList<EvDbMessage> messages,
        IEvDbStreamStoreData streamStore,
        DbConnection conn,
        DbTransaction transaction,
        CancellationToken cancellationToken)
    {
        #region messages = _transformers.Transform(...)

        foreach (var transformer in _transformers)
        {
            messages = messages.Select(m =>
            {
                var newPayload = transformer.Transform(m.Channel,
                                                       m.MessageType,
                                                       m.EventType,
                                                       m.Payload);

                return m with { Payload = newPayload };
            }).ToImmutableList();
        }

        #endregion //  messages = _transformers.Transform(...)

        string saveToTopicQuery = StreamQueries.SaveToOutbox;

        var shards = from message in messages
                     group (EvDbMessageRecord)message by message.ShardName;
        KeyValuePair<EvDbShardName, int>[] affctedCollection = IsSupportConcurrentCommands switch
        {
            true => await ExecuteInParallelAsync(),
            false => await ExecuteSequentialAsync(),
        };
        return affctedCollection.ToImmutableDictionary();

        #region ExecuteInParallelAsync

        async Task<KeyValuePair<EvDbShardName, int>[]> ExecuteInParallelAsync()
        {
            var tasks = shards.Select(async shard =>
            {
                EvDbShardName shardName = shard.Key;
                int affctedMessages = await ExecuteAsync(streamStore, conn, transaction, saveToTopicQuery, shard, shardName, cancellationToken);
                StoreMeters.AddMessages(affctedMessages, streamStore, DatabaseType, shard.Key);
                return KeyValuePair.Create(shardName, affctedMessages);
            });

            var affctedCollection = await Task.WhenAll(tasks);
            return affctedCollection;
        }

        #endregion //  ExecuteInParallelAsync

        #region ExecuteSequentialAsync

        async Task<KeyValuePair<EvDbShardName, int>[]> ExecuteSequentialAsync()
        {
            List<KeyValuePair<EvDbShardName, int>> results = new(); 
            foreach (var shard in shards)
            {
                EvDbShardName shardName = shard.Key;
                int affctedMessages = await ExecuteAsync(streamStore, conn, transaction, saveToTopicQuery, shard, shardName, cancellationToken);
                results.Add(KeyValuePair.Create(shardName, affctedMessages));
            }

            return results.ToArray();
        }

        #endregion //  ExecuteSequentialAsync

        #region ExecuteAsync

        async Task<int> ExecuteAsync(IEvDbStreamStoreData streamStore,
                                DbConnection conn,
                                DbTransaction transaction,
                                string saveToTopicQuery,
                                IGrouping<EvDbShardName, EvDbMessageRecord> shard,
                                EvDbShardName shardName,
                                CancellationToken cancellationToken)
        {
            string query = string.Format(saveToTopicQuery, shardName);
            EvDbMessageRecord[] items = shard.ToArray();

            OtelTags tags = OtelTags.Empty.Add("shard", shardName);
            using Activity? activity = _trace.StartActivity(tags, "EvDb.StoreOutboxAsync");
            int  affctedMessages = await OnStoreOutboxMessagesAsync(conn,
                                                                    shardName,
                                                                    query,
                                                                    items,
                                                                    transaction,
                                                                    cancellationToken);
            StoreMeters.AddMessages(affctedMessages, streamStore, DatabaseType, shard.Key);
            return affctedMessages;
        }

        #endregion //  ExecuteAsync
    }

    #endregion //  StoreOutboxAsync

    #region IEvDbStorageAdapter Members

    /// <summary>
    /// Tries to get a view's snapshot.
    /// </summary>
    /// <param name="viewAddress">The view address.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    async Task<EvDbStoredSnapshot> IEvDbStorageSnapshotAdapter.GetSnapshotAsync(
        EvDbViewAddress viewAddress,
        CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();

        string query = SnapshotQueries.GetSnapshot;
        _logger.LogQuery(query);

        var snapshot = await ExecuteSafe(conn =>
                                conn.QuerySingleOrDefaultAsync<EvDbStoredSnapshot>(query, viewAddress));
        if (snapshot == default)
            snapshot = EvDbStoredSnapshot.Empty;
        return snapshot;
    }

    async IAsyncEnumerable<EvDbEvent> IEvDbStorageStreamAdapter.GetEventsAsync(
        EvDbStreamCursor streamCursor,
        [EnumeratorCancellation] CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        string query = StreamQueries.GetEvents;
        _logger.LogQuery(query);

        using DbConnection conn = await InitAsync();
        DbDataReader reader = await conn.ExecuteReaderAsync(query, streamCursor);
        var parser = reader.GetRowParser<EvDbEventRecord>();
        while (await reader.ReadAsync())
        {
            EvDbEvent e = parser(reader);
            yield return e;
        }
    }

    async Task<StreamStoreAffected> IEvDbStorageStreamAdapter.StoreStreamAsync(
        IImmutableList<EvDbEvent> events,
        IImmutableList<EvDbMessage> messages,
        IEvDbStreamStoreData streamStore,
        CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        using DbConnection conn = await InitAsync();
        string saveEventsQuery = StreamQueries.SaveEvents;
        _logger.LogQuery(saveEventsQuery);

        EvDbEventRecord[] eventsRecords = events.Select<EvDbEvent, EvDbEventRecord>(e => e).ToArray();

        await using DbTransaction transaction = await conn.BeginTransactionAsync();
        try
        {
            int affctedEvents;
            IImmutableDictionary<EvDbShardName, int> affectedOnOutbox;

            if (IsSupportConcurrentCommands)
            {
                Task<int> affctedEventsTask = StoreEventsAsync();
                affectedOnOutbox = await StoreOutboxAsync();
                affctedEvents = await affctedEventsTask;
            }
            else
            { 
                affctedEvents = await StoreEventsAsync();
                affectedOnOutbox = await StoreOutboxAsync();
            }

            await transaction.CommitAsync();
            return new StreamStoreAffected(affctedEvents, affectedOnOutbox);
        }
        #region Exception Handling

        catch (Exception ex) when (IsOccException(ex))
        {
            await transaction.RollbackAsync();
            var cursor = new EvDbStreamCursor(streamStore.StreamAddress);
            throw new OCCException(cursor);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        #endregion //  Exception Handling

        #region StoreEventsAsync

        async Task<int> StoreEventsAsync()
        {
            int affctedEvents;
            using (_trace.StartActivity("EvDb.StoreEventsAsync"))
            {
                affctedEvents = await OnStoreStreamEventsAsync(conn,
                                                               saveEventsQuery,
                                                               eventsRecords,
                                                               transaction,
                                                               cancellation);
                StoreMeters.AddEvents(affctedEvents, streamStore, DatabaseType);
            }

            return affctedEvents;
        }

        #region StoreOutboxAsync

        async Task<IImmutableDictionary<EvDbShardName, int>> StoreOutboxAsync()
        {
            IImmutableDictionary<EvDbShardName, int> affectedOnOutbox =
                                    ImmutableDictionary<EvDbShardName, int>.Empty;
            if (messages.Count != 0)
            {
                affectedOnOutbox = await this.StoreOutboxAsync(messages,
                                                          streamStore,
                                                          conn,
                                                          transaction,
                                                          cancellation);
            }

            return affectedOnOutbox;
        }

        #endregion //  StoreOutboxAsync

        #endregion //  StoreEventsAsync
    }

    async Task IEvDbStorageSnapshotAdapter.StoreViewAsync(
        IEvDbViewStore viewStore,
        CancellationToken cancellation)
    {
        if (!viewStore.ShouldStoreSnapshot)
        {
            await Task.FromResult(true);
            return;
        }

        cancellation.ThrowIfCancellationRequested();
        string saveSnapshotQuery = SnapshotQueries.SaveSnapshot;
        _logger.LogQuery(saveSnapshotQuery);

        EvDbStoredSnapshotData snapshot = viewStore.GetSnapshotData();

        await ExecuteSafe(conn => OnStoreSnapshotAsync(conn, saveSnapshotQuery, snapshot, cancellation));
    }

    #endregion // IEvDbStorageAdapter Members

    /// <summary>
    /// Gets a value indicating whether this instance is support concurrent commands.
    /// </summary>
    protected virtual bool IsSupportConcurrentCommands { get; } = true; 

    #region IsOccException

    /// <summary>
    /// Determines whether  the exception is an occ exception.
    /// </summary>
    /// <param name="exception">The exception.</param>
    protected abstract bool IsOccException(Exception exception);

    #endregion //  IsOccException
}