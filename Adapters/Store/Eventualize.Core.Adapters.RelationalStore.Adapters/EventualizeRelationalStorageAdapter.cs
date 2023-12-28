using Dapper;
using Eventualize.Core.Abstractions;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

// TODO: [bnaya 2023-12-20] default timeout

namespace Eventualize.Core.Adapters;


// TODO: [bnaya 2023-12-19] all parameters and field should be driven from nameof or const
// TODO: [bnaya 2023-12-20] how do we get the domain?, shouldn't it be a parameter in each and every query?

/// <summary>
/// Store adapter for rational database
/// </summary>
/// <seealso cref="Eventualize.Core.IEventualizeStorageAdapter" />
public sealed class EventualizeRelationalStorageAdapter : IEventualizeStorageAdapter
{
    private readonly Task<DbConnection> _connectionTask;
    private readonly ILogger _logger;
    private readonly EventualizeAdapterQueryTemplates _queries;

    #region Create

    public static IEventualizeStorageAdapter Create(
        ILogger logger,
        EventualizeAdapterQueryTemplates queries,
        IEventualizeConnectionFactory factory)
    {
        return new EventualizeRelationalStorageAdapter(logger, queries, factory);
    }

    #endregion // Create

    #region Ctor

    private EventualizeRelationalStorageAdapter(
        ILogger logger,
        EventualizeAdapterQueryTemplates queryTemplates,
        IEventualizeConnectionFactory factory)
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

    #region IEventualizeStorageAdapter Members

    async Task<long> IEventualizeStorageAdapter.GetLastOffsetAsync<T>(EventualizeAggregate<T> aggregate, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        DbConnection conn = await _connectionTask;
        string query = _queries.GetLastSnapshotSnapshot;
        long offset = await conn.ExecuteScalarAsync<long>(query, aggregate.StreamUri);
        return offset;
    }

    /// <summary>
    /// Tries the get snapshot asynchronous.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="snapshotUri">The snapshot URI.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    async Task<EventualizeStoredSnapshot<T>?> IEventualizeStorageAdapter.TryGetSnapshotAsync<T>(
        EventualizeSnapshotUri snapshotUri, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        DbConnection conn = await _connectionTask;

        string query = _queries.TryGetSnapshot;

        var record = await conn.QuerySingleOrDefaultAsync<EventualizeeSnapshotRelationalRecrod>(query, snapshotUri) ?? throw new NoNullAllowedException("snapshot");
        var snapshot =  EventualizeStoredSnapshot.Create<T>(record);
        return snapshot;
    }

    async IAsyncEnumerable<IEventualizeStoredEvent> IEventualizeStorageAdapter.GetAsync(EventualizeStreamCursor parameter, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        DbConnection conn = await _connectionTask;
        string query = _queries.GetEvents;

        DbDataReader reader = await conn.ExecuteReaderAsync(query, parameter);
        var parser = reader.GetRowParser<EventualizeStoredEventEntity>();
        while (await reader.ReadAsync())
        {
            EventualizeStoredEvent e = parser(reader);
            yield return e;
        }
    }

    async Task IEventualizeStorageAdapter.SaveAsync<T>(EventualizeAggregate<T> aggregate, bool storeSnapshot, JsonSerializerOptions? options, CancellationToken cancellation)
    {
        SnapshotSaveParameter? snapshotSaveParameter = storeSnapshot ? SnapshotSaveParameter.Create(aggregate, options) : null;
        await SaveAsync(aggregate,  snapshotSaveParameter, cancellation);
    }

    async Task IEventualizeStorageAdapter.SaveAsync<T>(EventualizeAggregate<T> aggregate, bool storeSnapshot, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellation)
    {
        SnapshotSaveParameter? snapshotSaveParameter = storeSnapshot ? SnapshotSaveParameter.Create(aggregate, jsonTypeInfo) : null;
        await SaveAsync(aggregate, snapshotSaveParameter, cancellation);
    }

    // TODO: [bnaya 2023-12-13] avoid racing
    private async Task SaveAsync<T>(EventualizeAggregate<T> aggregate, SnapshotSaveParameter? snapshotSaveParameter, CancellationToken cancellation)
         where T : notnull, new()
    {
        cancellation.ThrowIfCancellationRequested();
        DbConnection conn = await _connectionTask;
        string query = _queries.Save;
        string snapQuery = _queries.SaveSnapshot;

        // TODO: [bnaya 2023-12-20] Thread safety (lock async) clear the pending on succeed?, transaction?,  
        try
        {
            var parameter = new AggregateSaveParameterCollection<T>(aggregate/*, domain? */);

            int affected = await conn.ExecuteAsync(query, parameter);
            if (snapshotSaveParameter != null)
            {
                int snapshot = await conn.ExecuteAsync(snapQuery, snapshotSaveParameter);
                if (snapshot != 1)
                    throw new DataException("Snapshot not saved");
            }
            // TODO: [bnaya 2023-12-20] do the logging right
            _logger.LogDebug("{count} events saved", affected);
        }
        catch (DbException e)
            when (e.Message.Contains("Violation of PRIMARY KEY constraint"))
        {
            throw new OCCException<T>(aggregate);
        }
    }

    #endregion // IEventualizeStorageAdapter Members

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

    ~EventualizeRelationalStorageAdapter()
    {
        ((IDisposable)this).Dispose();
    }

    #endregion // Dispose Pattern
}
