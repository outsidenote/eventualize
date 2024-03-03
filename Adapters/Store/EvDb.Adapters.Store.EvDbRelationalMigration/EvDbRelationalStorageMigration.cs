using Dapper;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace EvDb.Core.Adapters;

/// <summary>
/// Store adapter for rational database
/// </summary>
/// <seealso cref="EvDb.Core.IEvDbStorageAdapter" />
public abstract class EvDbRelationalStorageMigration : IEvDbStorageMigration
{
    private readonly Task<DbConnection> _commandsTask;

    #region Ctor

    protected EvDbRelationalStorageMigration(
        ILogger logger,
        IEvDbConnectionFactory factory)
    {
        _commandsTask = InitAsync();
        async Task<DbConnection> InitAsync()
        {
            DbConnection connection = factory.CreateConnection();
            await connection.OpenAsync();
            return connection;
        }
    }

    #endregion // Ctor

    protected abstract EvDbMigrationQueryTemplates Queries { get; }


    #region IEvDbStorageMigration Members

    async Task IEvDbStorageMigration.CreateEnvironmentAsync(CancellationToken cancellation)
    {
        var conn = await _commandsTask;

        string query = Queries.CreateEnvironment;
        await conn.ExecuteAsync(query);
    }

    async Task IEvDbStorageMigration.DestroyEnvironmentAsync(CancellationToken cancellation)
    {
        var conn = await _commandsTask;
        string query = Queries.DestroyEnvironment;

        await conn.ExecuteAsync(query);
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
