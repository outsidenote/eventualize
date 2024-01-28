using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;
using System.Text.Json;
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


    async Task<long> IEvDbStorageAdapter.GetLastOffsetAsync<T>(EvDbStreamAddress streamAddress, CancellationToken cancellation )
    {
        throw new NotImplementedException();
        //cancellation.ThrowIfCancellationRequested();
        //DbConnection conn = await _connectionTask;
        //string query = _queries.GetLastSnapshotSnapshot;
        //long offset = await conn.ExecuteScalarAsync<long>(query, aggregate.StreamAddress);
        //return offset;
    }

    /// <summary>
    /// Tries the get snapshot asynchronous.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="snapshotId">The snapshot id.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    async Task<EvDbStoredSnapshot> IEvDbStorageAdapter.TryGetSnapshotAsync(EvDbViewAddress viewAddress, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
        //cancellation.ThrowIfCancellationRequested();
        //DbConnection conn = await _connectionTask;

        //string query = _queries.TryGetSnapshot;

        //var record = await conn.QuerySingleOrDefaultAsync<EvDbeSnapshotRelationalRecrod>(query, snapshotId) ?? throw new NoNullAllowedException("snapshot");
        //var snapshot = EvDbStoredSnapshotFactory.Create<T>(record);
        //return snapshot;
    }

    async IAsyncEnumerable<IEvDbStoredEvent> IEvDbStorageAdapter.GetAsync(EvDbStreamCursor parameter, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        DbConnection conn = await _connectionTask;
        string query = _queries.GetEvents;

        DbDataReader reader = await conn.ExecuteReaderAsync(query, parameter);
        var parser = reader.GetRowParser<EvDbStoredEventRecord>();
        while (await reader.ReadAsync())
        {
            EvDbStoredEvent e = parser(reader);
            yield return e;
        }
    }

    async Task IEvDbStorageAdapter.SaveAsync(IEvDbStreamStoreData streamStore, CancellationToken cancellation)
    {
        using var tx = new TransactionScope();

        // TODO: Save Stream's events

        foreach (var view in streamStore.Views)
        {
            if (view.ShouldStoreSnapshot)
            { 
                // TODO: Save snapshots
            }
        }

        throw new NotImplementedException();
        //SnapshotSaveParameter? snapshotSaveParameter = storeSnapshot ? SnapshotSaveParameter.Create(aggregate, options) : null;
        //await SaveAsync(aggregate, snapshotSaveParameter, cancellation);
    }

    //async Task IEvDbStorageAdapter.SaveAsync<T>(EvDbCollectionMeta<T> aggregate, bool storeSnapshot, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellation)
    //{
    //    SnapshotSaveParameter? snapshotSaveParameter = storeSnapshot ? SnapshotSaveParameter.Create(aggregate, jsonTypeInfo) : null;
    //    await SaveAsync(aggregate, snapshotSaveParameter, cancellation);
    //}

    // TODO: [bnaya 2023-12-13] avoid racing
    //private async Task SaveAsync<T>(IEvDbStreamStore streamStore, IEnumerable<IEvDbView> views, SnapshotSaveParameter? snapshotSaveParameter, CancellationToken cancellation)
    //{
        //throw new NotImplementedException();
        //cancellation.ThrowIfCancellationRequested();
        //DbConnection conn = await _connectionTask;
        //string query = _queries.Save;
        //string snapQuery = _queries.SaveSnapshot;

        //// TODO: [bnaya 2023-12-20] Thread safety (lock async) clear the pending on succeed?, transaction?,  
        //try
        //{
        //    var parameter = new AggregateSaveParameterCollection<T>(aggregate/*, domain? */);

        //    int affected = await conn.ExecuteAsync(query, parameter);
        //    if (snapshotSaveParameter != null)
        //    {
        //        int snapshot = await conn.ExecuteAsync(snapQuery, snapshotSaveParameter);
        //        if (snapshot != 1)
        //            throw new DataException("Snapshot not saved");
        //    }
        //    // TODO: [bnaya 2023-12-20] do the logging right
        //    _logger.LogDebug("{count} events saved", affected);
        //}
        //catch (DbException e)
        //    when (e.Message.Contains("Violation of PRIMARY KEY constraint"))
        //{
        //    throw new OCCException(aggregate);
        //}
    //}

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
