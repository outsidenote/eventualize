using EvDb.Adapters.Store.SqlServer;
using EvDb.Core;
using EvDb.Core.Store.Internals;
using EvDb.IntegrationTests.EF;
using EvDb.IntegrationTests.EF.States;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Transactions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection extension methods for SqlServer Storage Adapter
/// </summary>
public static class PersonSnapshotStorageAdapterDI
{
    public static void UseSqlServerStoreForEvDbStream<TState>(
            this EvDbStreamStoreRegistrationContext instance,
            IEnumerable<IEvDbOutboxTransformer> transformers,
            string connectionStringOrConfigurationKey = "EvDbSqlServerConnection")
    {
        IServiceCollection services = instance.Services;
        EvDbPartitionAddress key = instance.Address;
        var context = instance.Context;
        services.AddKeyedScoped(
            key.ToString(),
            (sp, _) =>
            {
                var ctx = context
                    ?? sp.GetService<EvDbStorageContext>()
                    ?? EvDbStorageContext.CreateWithEnvironment("evdb");

                #region IEvDbConnectionFactory connectionFactory = ...

                string connectionString;
                IConfiguration? configuration = sp.GetService<IConfiguration>();
                connectionString = configuration?.GetConnectionString(connectionStringOrConfigurationKey) ?? connectionStringOrConfigurationKey;

                #endregion // IEvDbConnectionFactory connectionFactory = ...

                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<TState>();
                IEvDbStorageStreamAdapter adapter = EvDbSqlServerStorageAdapterFactory.CreateStreamAdapter(logger, connectionString, ctx, transformers);
                
                return adapter;
            });
    }

    public static void UseSqlServerForEvDbSnapshot<TState>(
            this EvDbSnapshotStoreRegistrationContext instance,
            string connectionStringOrConfigurationKey = "EvDbSqlServerConnection")
    {
        IServiceCollection services = instance.Services;
        EvDbViewBasicAddress key = instance.Address;
        var context = instance.Context;
        services.AddKeyedScoped<IEvDbStorageSnapshotAdapter>(
            key.ToString(),

            (sp, _) =>
            {
                var ctx = context
                    ?? sp.GetService<EvDbStorageContext>()
                    ?? EvDbStorageContext.CreateWithEnvironment("evdb");

                #region IEvDbConnectionFactory connectionFactory = ...

                IConfiguration? configuration = sp.GetService<IConfiguration>();
                string connectionString = configuration?.GetConnectionString(connectionStringOrConfigurationKey) ?? connectionStringOrConfigurationKey;

                #endregion // IEvDbConnectionFactory connectionFactory = ...

                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<TState>();
                IEvDbStorageSnapshotAdapter adapter = EvDbSqlServerStorageAdapterFactory.CreateSnapshotAdapter(logger, connectionString, ctx);
                return adapter;
            });
    }
}

public class EvDbTypedStorageStreamAdapter<TState> :
                        IEvDbStorageSnapshotAdapter<TState>
{
    private readonly PersonContext _context;
    private readonly IEvDbStorageSnapshotAdapter _adapter;
    private readonly Func<EvDbViewAddress, CancellationToken, Task<TState>> _getMapper;

    public EvDbTypedStorageStreamAdapter(
        PersonContext context,
        IEvDbStorageSnapshotAdapter adapter,
        Func<EvDbViewAddress,
        CancellationToken,
        Task<TState>> getMapper)
    {
        _context = context;
        _adapter = adapter;
        _getMapper = getMapper;
    }

    async Task<EvDbStoredSnapshot<TState>> IEvDbStorageSnapshotAdapter<TState>.GetSnapshotAsync(
        EvDbViewAddress viewAddress,
        CancellationToken cancellation)
    {
        //PersonEntity? entity = await _context.Persons.FirstOrDefaultAsync(p => p.Name == viewAddress.StreamId);
        //Person person = entity == null ? new Person() : entity.FromEntity();
        EvDbStoredSnapshot meta = await _adapter.GetSnapshotAsync(viewAddress, cancellation);
        TState state = await _getMapper(viewAddress, cancellation);
        return new EvDbStoredSnapshot<TState>(meta.Offset, state);
    }

    async Task IEvDbStorageSnapshotAdapter<TState>.StoreViewAsync(
        EvDbStoredSnapshotData<TState> data,
        CancellationToken cancellation)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        
        await _context.Persons.AddAsync((PersonEntity)(dynamic)(data.State));
        throw new NotImplementedException();
    }
}
