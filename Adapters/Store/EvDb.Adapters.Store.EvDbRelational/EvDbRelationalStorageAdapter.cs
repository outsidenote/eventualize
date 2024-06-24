// Ignore Spelling: Occ

using Dapper;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;

// TODO: [bnaya 2023-12-20] default timeout

namespace EvDb.Core.Adapters;

// TODO: [bnaya 2023-12-19] all parameters and field should be driven from nameof or const
// TODO: [bnaya 2023-12-20] how do we get the domain?, shouldn't it be a parameter in each and every query?

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
                conn?.Dispose();
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
    async Task<EvDbStoredSnapshot> IEvDbStorageViewAdapter.GetSnapshotAsync(
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
        IEvDbStreamStoreData streamData,
        CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        using DbConnection conn = await InitAsync();
        string saveEventsQuery = Queries.SaveEvents;
        _logger.LogQuery(saveEventsQuery);

        var eventsRecords = events.Select<EvDbEvent, EvDbEventRecord>(e => e).ToArray();

        using (var transaction = conn.BeginTransaction())
        {
            try
            {
                int affcted = await conn.ExecuteAsync(saveEventsQuery, eventsRecords, transaction);
                transaction.Commit();
                return affcted;
                // TODO: Bnaya 2024-06-10 add metrics affcted
            }
            catch (Exception ex) when (IsOccException(ex))
            {
                transaction.Rollback();
                throw new OCCException(streamData.Events.FirstOrDefault());
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }

    async Task IEvDbStorageViewAdapter.StoreViewAsync(IEvDbViewStore viewStore, CancellationToken cancellation)
    {
        if (!viewStore.ShouldStoreSnapshot)
        {
            await Task.FromResult(true);
            return;
        }

        cancellation.ThrowIfCancellationRequested();
        string saveSnapshotQuery = Queries.SaveSnapshot;
        _logger.LogQuery(saveSnapshotQuery);

        EvDbStoredSnapshotAddress snapshot = viewStore.GetSnapshot();

        // TODO: Bnaya 2024-06-24 add resiliency: "System.InvalidOperationException: ''"
        await ExecuteSafe(conn => conn.ExecuteAsync(saveSnapshotQuery, snapshot));
    }

    #endregion // IEvDbStorageAdapter Members

    protected abstract bool IsOccException(Exception exception);
}