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

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    protected virtual async void Dispose(bool disposed)
    {
        await Task.Yield();
        IDisposable commands = _commandsTask.Result;
        commands.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        var commands = await _commandsTask;
        await commands.DisposeAsync();
    }

    ~EvDbRelationalStorageMigration()
    {
        Dispose(false);
    }

    #endregion // Dispose Pattern
}
