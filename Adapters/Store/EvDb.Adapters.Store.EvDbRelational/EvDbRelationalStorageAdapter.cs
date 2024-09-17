// Ignore Spelling: Occ

using Dapper;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;

// TODO: [bnaya 2023-12-20] default timeout

namespace EvDb.Core.Adapters;

/// <summary>
/// Store adapter for rational database
/// </summary>
/// <seealso cref="EvDb.Core.IEvDbStorageAdapter" />
public abstract class EvDbRelationalStorageAdapter : IEvDbStorageAdapter
{
    protected readonly ILogger _logger;
    private readonly IEvDbConnectionFactory _factory;
    private const int RETRY_COUNT = 4;
    private const int RETRY_COUNT_DELAY_MILLI = 2;

    #region Ctor

    protected EvDbRelationalStorageAdapter(
        ILogger logger,
        IEvDbConnectionFactory factory)
    {
        _logger = logger;
        _factory = factory;
    }

    async Task<DbConnection> InitAsync()
    {
        DbConnection connection = _factory.CreateConnection();
        await connection.OpenAsync();
        return connection;
    }

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

    #endregion // Ctor

    protected abstract EvDbAdapterQueryTemplates Queries { get; }

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

        string query = Queries.GetSnapshot;
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
        string query = Queries.GetEvents;
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
        IImmutableList<EvDbOutboxEntity> outbox,
        IEvDbStreamStoreData streamStore,
        CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        using DbConnection conn = await InitAsync();
        string saveEventsQuery = Queries.SaveEvents;
        string saveToOutboxQuery = Queries.SaveToOutbox;
        _logger.LogQuery(saveEventsQuery);

        var eventsRecords = events.Select<EvDbEvent, EvDbEventRecord>(e => e).ToArray();

        using (var transaction = await conn.BeginTransactionAsync())
        {
            try
            {
                int affctedEvents = await conn.ExecuteAsync(saveEventsQuery, eventsRecords, transaction);
                if (outbox.Count != 0)
                {
                    var outboxRecords = outbox.Select<EvDbOutboxEntity, EvDbOutboxRecord>(e => e).ToArray();
                    int affctedOutbox = await conn.ExecuteAsync(saveToOutboxQuery, outboxRecords, transaction);
                    _logger.LogDebug("Affected outbox records = {Records}", affctedOutbox); // TODO: bnaya 2024-09-17 convert to hih-perf logging
                }
                await transaction.CommitAsync();
                return affctedEvents;
                // TODO: Bnaya 2024-06-10 add metrics affctedEvents
            }
            catch (Exception ex) when (IsOccException(ex))
            {
                await transaction.RollbackAsync();
                throw new OCCException(streamStore.Events.FirstOrDefault());
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
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
        string saveSnapshotQuery = Queries.SaveSnapshot;
        _logger.LogQuery(saveSnapshotQuery);

        EvDbStoredSnapshotData snapshot = viewStore.GetSnapshotData();

        await ExecuteSafe(conn => conn.ExecuteAsync(saveSnapshotQuery, snapshot));
    }

    #endregion // IEvDbStorageAdapter Members

    protected abstract bool IsOccException(Exception exception);
}