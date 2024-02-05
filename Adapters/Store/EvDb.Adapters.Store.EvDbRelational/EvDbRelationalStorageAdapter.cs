using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Transactions;

// TODO: [bnaya 2023-12-20] default timeout

namespace EvDb.Core.Adapters;


// TODO: [bnaya 2023-12-19] all parameters and field should be driven from nameof or const
// TODO: [bnaya 2023-12-20] how do we get the domain?, shouldn't it be a parameter in each and every query?

/// <summary>
/// Store adapter for rational database
/// </summary>
/// <seealso cref="EvDb.Core.IEvDbStorageAdapter" />
public sealed class EvDbRelationalStorageAdapter : IEvDbStorageAdapter
{
    private readonly Task<DbConnection> _connectionTask;
    private readonly ILogger _logger;
    private readonly EvDbAdapterQueryTemplates _queries;

    #region Create

    public static IEvDbStorageAdapter Create(
        ILogger logger,
        EvDbAdapterQueryTemplates queries,
        IEvDbConnectionFactory factory)
    {
        return new EvDbRelationalStorageAdapter(logger, queries, factory);
    }

    #endregion // Create

    #region Ctor

    private EvDbRelationalStorageAdapter(
        ILogger logger,
        EvDbAdapterQueryTemplates queryTemplates,
        IEvDbConnectionFactory factory)
    {
        _logger = logger;
        _queries = queryTemplates;
        _connectionTask = InitAsync();

        async Task<DbConnection> InitAsync()
        {
            DbConnection connection = factory.CreateConnection();
            await connection.OpenAsync();
            return connection;
        }
    }

    #endregion // Ctor

    #region IEvDbStorageAdapter Members

    /// <summary>
    /// Tries to get a view's snapshot.
    /// </summary>
    /// <param name="viewAddress">The view address.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    async Task<EvDbStoredSnapshot> IEvDbStorageAdapter.GetSnapshotAsync(
        EvDbViewAddress viewAddress,
        CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        DbConnection conn = await _connectionTask;

        string query = _queries.GetSnapshot;

        var snapshot = await conn.QuerySingleOrDefaultAsync<EvDbStoredSnapshot>(query, viewAddress);
        if (snapshot == default)
            snapshot = EvDbStoredSnapshot.Empty;
        return snapshot;
    }

    async IAsyncEnumerable<EvDbEvent> IEvDbStorageAdapter.GetEventsAsync(
                    EvDbStreamCursor streamCursor,
                    [EnumeratorCancellation]CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        DbConnection conn = await _connectionTask;
        string query = _queries.GetEvents;

        DbDataReader reader = await conn.ExecuteReaderAsync(query, streamCursor);
        var parser = reader.GetRowParser<EvDbEventRecord>();
        while (await reader.ReadAsync())
        {
            EvDbEvent e = parser(reader);
            yield return e;
        }
    }

    async Task IEvDbStorageAdapter.SaveAsync(IEvDbStreamStoreData streamData, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        DbConnection conn = await _connectionTask;
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        string saveEventsQuery = _queries.SaveEvents;
        string saveSnapshotQuery = _queries.SaveSnapshot;

        var events = streamData.Events.Select<EvDbEvent, EvDbEventRecord>(e => e).ToArray();
        await conn.ExecuteAsync(saveEventsQuery, events);

        foreach (IEvDbViewStore view in streamData.Views)
        {
            if (view.ShouldStoreSnapshot)
            {
                EvDbStoredSnapshotAddress snapshot = view.GetSnapshot();
                await conn.ExecuteAsync(saveSnapshotQuery, snapshot);
            }
        }

        tx.Complete();
    }


    #endregion // IEvDbStorageAdapter Members

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
