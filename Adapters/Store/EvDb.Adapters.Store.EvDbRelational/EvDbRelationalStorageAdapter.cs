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

    #region Ctor

    protected EvDbRelationalStorageAdapter(
        ILogger logger,
        IEvDbConnectionFactory factory)
    {
        _logger = logger;
        _connectionTask = InitAsync();

        async Task<DbConnection> InitAsync()
        {
            DbConnection connection = factory.CreateConnection();
            await connection.OpenAsync();
            return connection;
        }
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

        DbDataReader reader = await conn.ExecuteReaderAsync(query, streamCursor);
        var parser = reader.GetRowParser<EvDbEventRecord>();
        while (await reader.ReadAsync())
        {
            EvDbEvent e = parser(reader);
            yield return e;
        }
    }

    async Task IEvDbStorageStreamAdapter.SaveStreamAsync(
        IImmutableList<EvDbEvent> events, 
        IEvDbStreamStoreData streamData,
        CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        DbConnection conn = await _connectionTask;
        string saveEventsQuery = Queries.SaveEvents;

        var eventsRecords = events.Select<EvDbEvent, EvDbEventRecord>(e => e).ToArray();
        try
        {
            await conn.ExecuteAsync(saveEventsQuery, eventsRecords);
        }
        catch (Exception ex)
        {
            bool isOcc = IsOccException(ex);
            if (isOcc)
                throw new OCCException(streamData.Events.FirstOrDefault());
            throw;
        }
    }


    async Task IEvDbStorageViewAdapter.SaveViewAsync(IEvDbViewStore viewStore, CancellationToken cancellation)
    {
        if (!viewStore.ShouldStoreSnapshot)
        {
            await Task.FromResult(true);
            return;
        }

        cancellation.ThrowIfCancellationRequested();
        DbConnection conn = await _connectionTask;
        string saveSnapshotQuery = Queries.SaveSnapshot;

        EvDbStoredSnapshotAddress snapshot = viewStore.GetSnapshot();
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