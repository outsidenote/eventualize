using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Eventualize.Core.Adapters;

/// <summary>
/// Store adapter for rational database
/// </summary>
/// <seealso cref="Eventualize.Core.IEventualizeStorageAdapter" />
public sealed class EventualizeRelationalStorageAdapter : IEventualizeStorageAdapter
{
    private readonly Task<Commands> _commandsTask;

    public static IEventualizeStorageAdapter Create(
        ILogger logger,
        EventualizeAdapterQueryTemplates queryTemplates,
        IEventualizeConnectionFactory factory)
    {
        return new EventualizeRelationalStorageAdapter(logger, queryTemplates, factory);
    }

    #region Ctor

    private EventualizeRelationalStorageAdapter(
        ILogger logger,
        EventualizeAdapterQueryTemplates queryTemplates,
        IEventualizeConnectionFactory factory)
    {
        _commandsTask = InitAsync();
        async Task<Commands> InitAsync()
        {
            DbConnection connection = factory.CreateConnection();
            var commands = new Commands(connection, queryTemplates);
            await connection.OpenAsync();
            return commands;
        }
    }

    #endregion // Ctor

    #region IEventualizeStorageAdapter Members

    async Task<long> IEventualizeStorageAdapter.GetLastSequenceIdAsync<T>(EventualizeAggregate<T> aggregate)
    {
        var commands = await _commandsTask;
        DbCommand command = commands.GetLastSnapshotSequenceId;

        var type = command.CreateParameter();
        type.DbType = DbType.String;
        type.IsNullable = false;
        type.Value = aggregate.AggregateType.Name;
        type.ParameterName = EventualizeAdapterParametersConstants.type;

        var id = command.CreateParameter();
        id.DbType = DbType.String;
        id.IsNullable = false;
        id.Value = aggregate.Id;
        id.ParameterName = EventualizeAdapterParametersConstants.id;

        DbDataReader reader = await command.ExecuteReaderAsync();
        if (! await reader.ReadAsync())
            return -1;
        var sequenceId = reader.GetInt64(0);
        return sequenceId;
    }

    async Task<EventualizeStoredSnapshotData<T>?> IEventualizeStorageAdapter.TryGetSnapshotAsync<T>(
        string aggregateTypeName,
        string id)
    {
        var commands = await _commandsTask;
        DbCommand command = commands.TryGetSnapshot;
        // TODO: [bnaya 2023-12-13] set parameters

        DbDataReader reader = await command.ExecuteReaderAsync();
        if(!await reader.ReadAsync())
            return null;
        var jsonData = reader.GetString(0);
        var sequenceId = reader.GetInt64(1);
        var snapshot = JsonSerializer.Deserialize<T>(jsonData); 
        if (snapshot == null)
            return default;
        return new EventualizeStoredSnapshotData<T>(snapshot, sequenceId);
    }

    async IAsyncEnumerable<EventualizeEvent> IEventualizeStorageAdapter.GetAsync(
                            string aggregateTypeName,
                            string id,
                            long startSequenceId)
    {
        var commands = await _commandsTask;
        DbCommand command = commands.GetEvents;
        // TODO: [bnaya 2023-12-13] set parameters

        DbDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            // TODO: [bnaya 2023-12-13] get by name
            var eventType = reader.GetString(0);
            var capturedAt = reader.GetDateTime(1);
            var capturedBy = reader.GetString(2);
            var jsonData = reader.GetString(3);
            var storedAt = reader.GetDateTime(4);
            var e = new EventualizeEvent(eventType, capturedAt, capturedBy, jsonData, storedAt);
            yield return e;
        }
    }

    // TODO: [bnaya 2023-12-13] avoid racing
    async Task<IImmutableList<EventualizeEvent>> IEventualizeStorageAdapter.SaveAsync<T>(EventualizeAggregate<T> aggregate, bool storeSnapshot)
    {
        var commands = await _commandsTask;
        DbCommand command = commands.Save;

        // TODO: [bnaya 2023-12-13] set parameters
        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (DataException e)
        {
            if (e.Message.Contains("Violation of PRIMARY KEY constraint"))
                throw new OCCException<T>(aggregate);
        }
        return aggregate.PendingEvents;
    }

    #endregion // IEventualizeStorageAdapter Members

    #region Dispose Pattern

    void IDisposable.Dispose()
    {
        IDisposable commands = _commandsTask.Result;
        commands.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        var commands = await _commandsTask;
        await commands.DisposeAsync();
    }

    ~EventualizeRelationalStorageAdapter()
    {
        ((IDisposable)this).Dispose();
    }

    #endregion // Dispose Pattern

    #region class Commands

    private sealed class Commands: IDisposable, IAsyncDisposable
    {
        private readonly DbConnection _connection;

        public Commands(
            DbConnection connection,
            EventualizeAdapterQueryTemplates queryTemplates)
        {
            GetLastSnapshotSequenceId = connection.CreateCommand();
            GetLastSnapshotSequenceId.CommandText = queryTemplates.GetLastSnapshotSequenceId;

            TryGetSnapshot = connection.CreateCommand();
            TryGetSnapshot.CommandText = queryTemplates.TryGetSnapshot;

            GetEvents = connection.CreateCommand();
            GetEvents.CommandText = queryTemplates.GetEvents;

            Save = connection.CreateCommand();
            Save.CommandText = queryTemplates.Save;
            _connection = connection;
        }

        /// <summary>
        /// Get last snapshot sequence identifier.
        /// </summary>
        public DbCommand GetLastSnapshotSequenceId { get;  }
        /// <summary>
        /// Get latest snapshot.
        /// </summary>
        public DbCommand TryGetSnapshot { get;  }
        /// <summary>
        /// Get events.
        /// </summary>
        public DbCommand GetEvents { get;  }
        /// <summary>
        /// Save.
        /// </summary>
        public DbCommand Save { get; }

        #region Dispose Pattern

        void IDisposable.Dispose()
        {
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            await _connection.DisposeAsync();
        }

        #endregion // Dispose Pattern
    }

    #endregion // class Commands
}
