// Ignore Spelling: Occ

using Dapper;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
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

    async Task<int> IEvDbStorageStreamAdapter.StoreStreamAsync(
        IImmutableList<EvDbEvent> events,
        IImmutableList<EvDbMessage> messages,
        IEvDbStreamStoreData streamStore,
        CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        using DbConnection conn = await InitAsync();
        string saveEventsQuery = StreamQueries.SaveEvents;
        string saveToTopicQuery = StreamQueries.SaveToTopics;
        _logger.LogQuery(saveEventsQuery);

        var eventsRecords = events.Select<EvDbEvent, EvDbEventRecord>(e => e).ToArray();


        foreach (var transformer in _transformers)
        {
            messages = messages.Select(m =>
            {
                var newPayload = transformer.Transform(m.Channel, m.MessageType, m.EventType, m.Payload);

                return m with { Payload = newPayload };
            }).ToImmutableList();
        }

        await using var transaction = await conn.BeginTransactionAsync();
        try
        {
            int affctedEvents = await conn.ExecuteAsync(saveEventsQuery, eventsRecords, transaction);
            StoreMeters.AddEvents(affctedEvents, streamStore, DatabaseType);
            if (messages.Count != 0)
            {
                var tables = from message in messages
                             group (EvDbMessageRecord)message by message.ShardName;
                foreach (var table in tables)
                {
                    string query = string.Format(saveToTopicQuery, table.Key);
                    var items = table.ToArray();
                    int affctedMessages = await conn.ExecuteAsync(query, items, transaction);
                    StoreMeters.AddMessages(affctedMessages, streamStore, DatabaseType, table.Key);
                }
            }
            await transaction.CommitAsync();
            return affctedEvents;
        }
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
    }

    async Task IEvDbStorageSnapshotAdapter.StoreViewAsync(IEvDbViewStore viewStore, CancellationToken cancellation)
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

        await ExecuteSafe(conn => conn.ExecuteAsync(saveSnapshotQuery, snapshot));
    }

    #endregion // IEvDbStorageAdapter Members

    #region IsOccException

    /// <summary>
    /// Determines whether  the exception is an occ exception.
    /// </summary>
    /// <param name="exception">The exception.</param>
    protected abstract bool IsOccException(Exception exception);

    #endregion //  IsOccException
}