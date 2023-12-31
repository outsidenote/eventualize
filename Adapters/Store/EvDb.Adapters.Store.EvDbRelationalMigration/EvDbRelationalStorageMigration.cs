using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace EvDb.Core.Adapters;

/// <summary>
/// Store adapter for rational database
/// </summary>
/// <seealso cref="EvDb.Core.IEvDbStorageAdapter" />
public sealed class EvDbRelationalStorageMigration : IEvDbStorageMigration
{
    private readonly Task<Commands> _commandsTask;

    public static IEvDbStorageMigration Create(
        ILogger logger,
        EvDbMigrationQueryTemplates queryTemplates,
        IEvDbConnectionFactory factory)
    {
        return new EvDbRelationalStorageMigration(logger, queryTemplates, factory);
    }

    #region Ctor

    private EvDbRelationalStorageMigration(
        ILogger logger,
        EvDbMigrationQueryTemplates queryTemplates,
        IEvDbConnectionFactory factory)
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

    #region IEvDbStorageMigration Members

    async Task IEvDbStorageMigration.CreateTestEnvironmentAsync(CancellationToken cancellation)
    {
        var commands = await _commandsTask;
        DbCommand command = commands.CreateEnvironment;

        await command.ExecuteNonQueryAsync();
    }

    async Task IEvDbStorageMigration.DestroyTestEnvironmentAsync(CancellationToken cancellation)
    {
        var commands = await _commandsTask;
        DbCommand command = commands.DestroyEnvironment;

        await command.ExecuteNonQueryAsync();
    }

    #endregion // IEvDbStorageMigration Members

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

    ~EvDbRelationalStorageMigration()
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
            EvDbMigrationQueryTemplates queryTemplates)
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
