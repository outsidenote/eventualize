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
    private readonly Task<DbConnection> _connectionTask;
    protected readonly ILogger _logger;
    private readonly IEvDbConnectionFactory _factory;

    #region Ctor

    protected EvDbRelationalStorageAdapter(
        ILogger logger,
        IEvDbConnectionFactory factory)
    {
        _logger = logger;
        _factory = factory;
        _connectionTask = InitAsync();
    }

    async Task<DbConnection> InitAsync()
    {
        DbConnection connection = _factory.CreateConnection();
        await connection.OpenAsync();
        return connection;
    }

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
        DbConnection conn = await _connectionTask;

        string query = Queries.GetSnapshot;
        _logger.LogQuery(query);

        var snapshot = await conn.QuerySingleOrDefaultAsync<EvDbStoredSnapshot>(query, viewAddress);
        if (snapshot == default)
            snapshot = EvDbStoredSnapshot.Empty;
        return snapshot;
    }

    async IAsyncEnumerable<EvDbEvent> IEvDbStorageStreamAdapter.GetEventsAsync(
        EvDbStreamCursor streamCursor,
        [EnumeratorCancellation] CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        DbConnection conn = await _connectionTask;
        string query = Queries.GetEvents;
        _logger.LogQuery(query);

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

        // TODO: Bnaya 2024-06-24 add resiliency: "System.InvalidOperationException: 'Invalid operation. The connection is closed.'"
        DbConnection conn = await _connectionTask;
        await conn.ExecuteAsync(saveSnapshotQuery, snapshot);
    }

    #endregion // IEvDbStorageAdapter Members

    protected abstract bool IsOccException(Exception exception);

    #region Dispose Pattern

    void IDisposable.Dispose()
    {
        IDisposable commands = _connectionTask.Result;
        commands.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        var commands = await _connectionTask;
        await commands.DisposeAsync();
    }

    ~EvDbRelationalStorageAdapter()
    {
        ((IDisposable)this).Dispose();
    }

    #endregion // Dispose Pattern
}