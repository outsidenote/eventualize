// Ignore Spelling: Occ

using Dapper;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Transactions;
using static EvDb.Core.Adapters.StoreTelemetry;

namespace EvDb.Core.Adapters;

/// <summary>
/// Store adapter for rational database
/// </summary>
/// <seealso cref="EvDb.Core.IEvDbStorageAdapter" />
public abstract class EvDbRelationalStorageAdapter :
    IEvDbStorageStreamAdapter,
    IEvDbStorageSnapshotAdapter,
    IEvDbRecordParserFactory
{
    private readonly static ActivitySource _trace = StoreTelemetry.Trace;
    protected readonly ILogger _logger;
    private readonly IEvDbConnectionFactory _factory;
    private readonly IImmutableList<IEvDbOutboxTransformer> _transformers;
    private const int RETRY_COUNT = 4;
    private const int RETRY_COUNT_DELAY_MILLI = 2;
    private static readonly TimeSpan TX_TIMEOUT = TimeSpan.FromSeconds(5);

    #region Ctor

    protected EvDbRelationalStorageAdapter(ILogger logger,
        IEvDbConnectionFactory factory,
        IEnumerable<IEvDbOutboxTransformer> transformers)
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
    /// Gets the snapshotData's queries.
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
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task<int> OnStoreStreamEventsAsync(
        DbConnection connection,
        string query,
        EvDbEventRecord[] records,
        CancellationToken cancellationToken)
    {
        int affctedEvents = await connection.ExecuteAsync(query, records);
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
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task<int> OnStoreOutboxMessagesAsync(
        DbConnection connection,
        EvDbShardName shardName,
        string query,
        EvDbMessageRecord[] records,
        CancellationToken cancellationToken)
    {
        int affctedMessages = await connection.ExecuteAsync(query, records);
        return affctedMessages;
    }

    #endregion //  OnStoreOutboxMessagesAsync

    #region OnGetSnapshotAsync

    /// <summary>
    /// Gets the snapshotData.
    /// </summary>
    /// <param name="viewAddress">The view uniqueness.</param>
    /// <param name="conn">The connection.</param>
    /// <param name="query">The query.</param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    protected virtual async Task<EvDbStoredSnapshot> OnGetSnapshotAsync(
        EvDbViewAddress viewAddress,
        DbConnection conn,
        string query,
        CancellationToken cancellation)
    {
        EvDbStoredSnapshot? result =
                       await conn.QuerySingleOrDefaultAsync<EvDbStoredSnapshot>(
                                                query,
                                                viewAddress);
        return result ?? EvDbStoredSnapshot.Empty;
    }

    #endregion //  OnGetSnapshotAsync

    #region OnStoreSnapshotAsync

    /// <summary>
    /// Store a snapshotData.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="query">The save snapshotData query.</param>
    /// <param name="snapshot">The snapshotData data.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async virtual Task<int> OnStoreSnapshotAsync(DbConnection connection,
                                              string query,
                                              EvDbStoredSnapshotData snapshot,
                                              CancellationToken cancellationToken)
    {
        int affected = await connection.ExecuteAsync(query, snapshot);
        return affected;
    }

    #endregion //  OnStoreSnapshotAsync

    #region StoreOutboxAsync

    private async Task<IImmutableDictionary<EvDbShardName, int>> StoreOutboxAsync(
        IImmutableList<EvDbMessage> messages,
        DbConnection conn,
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

        string saveToOutboxQuery = StreamQueries.SaveToOutbox;

        EvDbStreamAddress address = messages[0].StreamCursor;
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
                int affctedMessages = await ExecuteAsync(conn, saveToOutboxQuery, shard, shardName, cancellationToken);
                StoreMeters.AddMessages(affctedMessages, address, DatabaseType, shard.Key);
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
                int affctedMessages = await ExecuteAsync(conn, saveToOutboxQuery, shard, shardName, cancellationToken);
                results.Add(KeyValuePair.Create(shardName, affctedMessages));
            }

            return results.ToArray();
        }

        #endregion //  ExecuteSequentialAsync

        #region ExecuteAsync

        async Task<int> ExecuteAsync(
                                DbConnection conn,
                                string saveToOutboxQuery,
                                IGrouping<EvDbShardName, EvDbMessageRecord> shard,
                                EvDbShardName shardName,
                                CancellationToken cancellationToken)
        {
            string query = string.Format(saveToOutboxQuery, shardName);
            EvDbMessageRecord[] items = shard.ToArray();

            OtelTags tags = OtelTags.Empty.Add("shard", shardName);
            using Activity? activity = _trace.StartActivity(tags, "EvDb.StoreOutboxAsync");
            int affctedMessages = await OnStoreOutboxMessagesAsync(conn,
                                                                    shardName,
                                                                    query,
                                                                    items,
                                                                    cancellationToken);
            StoreMeters.AddMessages(affctedMessages, address, DatabaseType, shard.Key);
            return affctedMessages;
        }

        #endregion //  ExecuteAsync
    }

    #endregion //  StoreOutboxAsync

    #region IsSupportConcurrentCommands

    /// <summary>
    /// Gets a value indicating whether this instance is support concurrent commands.
    /// </summary>
    protected virtual bool IsSupportConcurrentCommands { get; } = true;

    #endregion //  IsSupportConcurrentCommands

    #region RecordParserFactory

    /// <summary>
    /// Gets the record parser factory.
    /// </summary>
    protected virtual IEvDbRecordParserFactory RecordParserFactory => this;

    #endregion //  RecordParserFactory

    #region IEvDbRecordParserFactory members

    /// <summary>
    /// Creates the specified reader.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns></returns>
    IEvDbRecordParser IEvDbRecordParserFactory.Create(DbDataReader reader) => new RecordParser(reader);

    #endregion //  IEvDbRecordParserFactory members

    #region class RecordParser

    private sealed class RecordParser : IEvDbRecordParser
    {
        private readonly Func<DbDataReader, EvDbEventRecord> _parser;
        private readonly DbDataReader _reader;

        public RecordParser(DbDataReader reader)
        {
            _parser = reader.GetRowParser<EvDbEventRecord>();
            _reader = reader;
        }

        EvDbEventRecord IEvDbRecordParser.ParseEvent() => _parser(_reader);
    }

    #endregion //  class RecordParser

    #region IEvDbStorageStreamAdapter Members

    async IAsyncEnumerable<EvDbEvent> IEvDbStorageStreamAdapter.GetEventsAsync(
        EvDbStreamCursor streamCursor,
        [EnumeratorCancellation] CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        string query = StreamQueries.GetEvents;
        _logger.LogQuery(query);

        using DbConnection conn = await InitAsync();
        DbDataReader reader = await conn.ExecuteReaderAsync(query, streamCursor);
        var parser = RecordParserFactory.Create(reader);
        while (await reader.ReadAsync())
        {
            EvDbEvent e = parser.ParseEvent();
            yield return e;
        }
    }

    async Task<StreamStoreAffected> IEvDbStorageStreamAdapter.StoreStreamAsync(
        IImmutableList<EvDbEvent> events,
        IImmutableList<EvDbMessage> messages,
        CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        using DbConnection conn = await InitAsync();
        string saveEventsQuery = StreamQueries.SaveEvents;
        _logger.LogQuery(saveEventsQuery);

        EvDbEventRecord[] eventsRecords = events.Select<EvDbEvent, EvDbEventRecord>(e => e).ToArray();

        try
        {
            using var tx = new TransactionScope(TransactionScopeOption.Required,
                                                TX_TIMEOUT,
                                                TransactionScopeAsyncFlowOption.Enabled);
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

            tx.Complete();
            return new StreamStoreAffected(affctedEvents, affectedOnOutbox);
        }
        #region Exception Handling

        catch (Exception ex) when (IsOccException(ex))
        {
            var address = events[0].StreamCursor;
            var cursor = new EvDbStreamCursor(address);
            throw new OCCException(cursor);
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
                                                               cancellation);
                EvDbStreamAddress address = events[0].StreamCursor;
                StoreMeters.AddEvents(affctedEvents, address, DatabaseType);
            }

            return affctedEvents;
        }

        #endregion //  StoreEventsAsync

        #region StoreOutboxAsync

        async Task<IImmutableDictionary<EvDbShardName, int>> StoreOutboxAsync()
        {
            IImmutableDictionary<EvDbShardName, int> affectedOnOutbox =
                                    ImmutableDictionary<EvDbShardName, int>.Empty;
            if (messages.Count != 0)
            {
                affectedOnOutbox = await this.StoreOutboxAsync(messages,
                                                          conn,
                                                          cancellation);
            }

            return affectedOnOutbox;
        }

        #endregion //  StoreOutboxAsync
    }

    #endregion // IEvDbStorageStreamAdapter Members

    #region IEvDbStorageSnapshotAdapter Members

    /// <summary>
    /// Tries to get a view's snapshotData.
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

        EvDbStoredSnapshot snapshot = await ExecuteSafe(conn => OnGetSnapshotAsync(viewAddress, conn, query, cancellation));
        return snapshot;
    }

    async Task IEvDbStorageSnapshotAdapter.StoreSnapshotAsync(
        EvDbStoredSnapshotData snapshotData,
        CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        string saveSnapshotQuery = SnapshotQueries.SaveSnapshot;
        _logger.LogQuery(saveSnapshotQuery);


        await ExecuteSafe(conn => OnStoreSnapshotAsync(conn, saveSnapshotQuery, snapshotData, cancellation));
    }

    #endregion // IEvDbTypedStorageSnapshotAdapter Members

    #region IsOccException

    /// <summary>
    /// Determines whether  the exception is an occ exception.
    /// </summary>
    /// <param name="exception">The exception.</param>
    protected abstract bool IsOccException(Exception exception);

    #endregion //  IsOccException
}