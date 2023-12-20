using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace Eventualize.Core.Adapters;

/// <summary>
/// Store adapter for rational database
/// </summary>
/// <seealso cref="Eventualize.Core.IEventualizeStorageAdapter" />
public sealed class EventualizeRelationalStorageMigration : IEventualizeStorageMigration
{
    private readonly Task<Commands> _commandsTask;

    public static IEventualizeStorageMigration Create(
        ILogger logger,
        EventualizeMigrationQueryTemplates queryTemplates,
        IEventualizeConnectionFactory factory)
    {
        return new EventualizeRelationalStorageMigration(logger, queryTemplates, factory);
    }

    #region Ctor

    private EventualizeRelationalStorageMigration(
        ILogger logger,
        EventualizeMigrationQueryTemplates queryTemplates,
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

    #region IEventualizeStorageMigration Members

    async Task IEventualizeStorageMigration.CreateTestEnvironmentAsync()
    {
        var commands = await _commandsTask;
        DbCommand command = commands.CreateEnvironment;

        await command.ExecuteNonQueryAsync();
    }

    async Task IEventualizeStorageMigration.DestroyTestEnvironmentAsync()
    {
        var commands = await _commandsTask;
        DbCommand command = commands.DestroyEnvironment;

        await command.ExecuteNonQueryAsync();
    }

    #endregion // IEventualizeStorageMigration Members

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

    ~EventualizeRelationalStorageMigration()
    {
        ((IDisposable)this).Dispose();
    }

    #endregion // Dispose Pattern

    #region class Commands

    private sealed class Commands : IDisposable, IAsyncDisposable
    {
        private readonly DbConnection _connection;

        public Commands(
            DbConnection connection,
            EventualizeMigrationQueryTemplates queryTemplates)
        {
            CreateEnvironment = connection.CreateCommand();
            CreateEnvironment.CommandText = queryTemplates.CreateEnvironment;

            DestroyEnvironment = connection.CreateCommand();
            DestroyEnvironment.CommandText = queryTemplates.DestroyEnvironment;

            _connection = connection;
        }

        /// <summary>
        /// Get last snapshot sequence identifier.
        /// </summary>
        public DbCommand CreateEnvironment { get; }
        /// <summary>
        /// Get latest snapshot.
        /// </summary>
        public DbCommand DestroyEnvironment { get; }

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
