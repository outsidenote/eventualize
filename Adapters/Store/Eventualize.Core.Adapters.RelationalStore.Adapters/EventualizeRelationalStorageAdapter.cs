using Dapper;
using Eventualize.Core;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text.Json;

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

    async Task<EventualizeStoredSnapshot<T>?> IEventualizeStorageAdapter.TryGetSnapshotAsync<T>(
        EventualizeSnapshotUri snapshotUri, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        DbConnection conn = await _connectionTask;

        string query = _queries.TryGetSnapshot;

        var record = await conn.QuerySingleOrDefaultAsync<EventualizeeSnapshotRelationalRecrod>(query, snapshotUri);
        if (record == null)
            return null;
        return EventualizeStoredSnapshot<T>.Create(record);
    }

    async IAsyncEnumerable<EventualizeStoredEvent> IEventualizeStorageAdapter.GetAsync(EventualizeStreamCursor parameter, CancellationToken cancellation)
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

    // TODO: [bnaya 2023-12-13] avoid racing
    async Task IEventualizeStorageAdapter.SaveAsync<T>(EventualizeAggregate<T> aggregate, bool storeSnapshot, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        DbConnection conn = await _connectionTask;
        string query = _queries.Save;
        string snapQuery = _queries.SaveSnapshot;

        // TODO: [bnaya 2023-12-20] Thread safety (lock async) clear the pending on succeed?, transaction?,  
        // TODO: [bnaya 2023-12-13] set parameters
        try
        {
            var parameter = new AggregateSaveParameterCollection<T>(aggregate/*, domain? */);

            int affected = await conn.ExecuteAsync(query, parameter);
            if (storeSnapshot)
            {
                var offset = aggregate.LastStoredOffset + parameter.Events.Count;
                // TODO: [bnaya 2023-12-20] serialization?
                // TODO: [bnaya 2023-12-20] domain
                var payload = JsonSerializer.Serialize(aggregate.State);
                SnapshotSaveParameter snapshotSaveParameter = new SnapshotSaveParameter(
                                            aggregate.StreamUri.StreamId,
                                            aggregate.StreamUri.StreamType,
                                            offset,
                                            payload,
                                            "default");
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
