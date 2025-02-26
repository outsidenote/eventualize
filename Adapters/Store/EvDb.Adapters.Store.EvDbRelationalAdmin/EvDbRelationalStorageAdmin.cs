// Ignore Spelling: Admin

using Dapper;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace EvDb.Core.Adapters;

/// <summary>
/// Store adapter for rational database
/// </summary>
/// <seealso cref="EvDb.Core.IEvDbStorageAdapter" />
internal sealed class EvDbRelationalStorageAdmin : IEvDbStorageAdmin
{
    private readonly Task<DbConnection> _commandsTask;
    private readonly ILogger _logger;
    private readonly EvDbMigrationQueryTemplates _scripts;

    #region Ctor

    public EvDbRelationalStorageAdmin(
        ILogger logger,
        IEvDbConnectionFactory factory,
        EvDbMigrationQueryTemplates scripts)
    {
        _commandsTask = InitAsync();
        async Task<DbConnection> InitAsync()
        {
            DbConnection connection = factory.CreateConnection();
            await connection.OpenAsync();
            return connection;
        }

        _logger = logger;
        _scripts = scripts;
    }

    #endregion // Ctor

    #region IEvDbStorageAdmin Members

    async Task IEvDbStorageAdmin.CreateEnvironmentAsync(CancellationToken cancellation)
    {
        var conn = await _commandsTask;

        foreach (string query in _scripts.CreateEnvironment)
        {
            _logger.LogInformation(query);
            await conn.ExecuteAsync(query);
        }
    }

    async Task IEvDbStorageAdmin.DestroyEnvironmentAsync(CancellationToken cancellation)
    {
        var conn = await _commandsTask;
        string query = _scripts.DestroyEnvironment;
        _logger.LogInformation(query);

        await conn.ExecuteAsync(query);
    }

    EvDbMigrationQueryTemplates IEvDbStorageAdmin.Scripts => _scripts;


    #endregion // IEvDbStorageAdmin Members

    #region Dispose Pattern

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        IDisposable commands = _commandsTask.Result;
        commands.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        var commands = await _commandsTask;
        await commands.DisposeAsync();
    }

    ~EvDbRelationalStorageAdmin()
    {
        Dispose();
    }

    #endregion // Dispose Pattern
}
