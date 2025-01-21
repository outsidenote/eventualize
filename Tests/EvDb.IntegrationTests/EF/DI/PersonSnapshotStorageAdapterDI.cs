using Castle.Core.Configuration;
using EvDb.Adapters.Store.SqlServer;
using EvDb.Core;
using EvDb.Core.Store.Internals;
using EvDb.IntegrationTests.EF;
using EvDb.IntegrationTests.EF.States;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Transactions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Microsoft.Extensions.DependencyInjection;

public readonly record struct TypedStorageOptions
{
    public static readonly TypedStorageOptions Default = new TypedStorageOptions
    {
        EvDbConnectionStringOrConfigurationKey = "EvDbSqlServerConnection"
    };

    public string EvDbConnectionStringOrConfigurationKey { get; init; }
    public string? ContextConnectionStringOrConfigurationKey { get; init; }
    public int? CommandTimeout { get; init; }
}

/// <summary>
/// Dependency injection extension methods for SqlServer Storage Adapter
/// </summary>
public static class PersonSnapshotStorageAdapterDI
{
    public static void UseSqlServerForEvDbSnapshot<TState>(
            this EvDbSnapshotStoreRegistrationContext instance,
            Func<TypedStorageOptions, TypedStorageOptions>? options)
    {
        TypedStorageOptions setting = options?.Invoke(TypedStorageOptions.Default) ?? TypedStorageOptions.Default;
        IServiceCollection services = instance.Services;
        EvDbViewBasicAddress key = instance.Address;
        services.AddDbContextFactory<PersonContext>(
        optionsBuilder =>
        {
            string? connectionString = setting.ContextConnectionStringOrConfigurationKey;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string not found.");
            }

            optionsBuilder.UseSqlServer(connectionString, sqlServerOptions =>
            {
                sqlServerOptions.CommandTimeout(setting.CommandTimeout);
                //sqlServerOptions.EnableRetryOnFailure(setting., TimeSpan.FromSeconds(dbResilienceSettings.MaxRetryDelaySeconds), null);
            });
        });

        var context = instance.Context;
        services.AddKeyedScoped<IEvDbStorageSnapshotAdapter<Person>>(
            key.ToString(),

            (sp, _) =>
            {
                var ctx = context
                    ?? sp.GetService<EvDbStorageContext>()
                    ?? EvDbStorageContext.CreateWithEnvironment("evdb");

                #region IEvDbConnectionFactory connectionFactory = ...

                string connectionString = setting.EvDbConnectionStringOrConfigurationKey;

                #endregion // IEvDbConnectionFactory connectionFactory = ...

                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<TState>();
                IEvDbStorageSnapshotAdapter adapter = EvDbSqlServerStorageAdapterFactory.CreateSnapshotAdapter(logger, connectionString, ctx);
                PersonContext personContext = sp.GetRequiredService<IDbContextFactory<PersonContext>>()
                                                .CreateDbContext();
                var typedAdapter = new EvDbPersonStorageStreamAdapter(personContext, adapter);
                return typedAdapter;
            });
    }
}

public class EvDbPersonStorageStreamAdapter : EvDbTypedStorageStreamAdapter<Person>
{
    private readonly PersonContext _context;

    public EvDbPersonStorageStreamAdapter(PersonContext context,
                                          IEvDbStorageSnapshotAdapter adapter) : base(adapter)
    {
        _context = context;
    }

    async protected override Task<Person> OnGetSnapshotAsync(
        EvDbViewAddress viewAddress,
        EvDbStoredSnapshot metadata,
        CancellationToken cancellation)
    {
        int personId = JsonSerializer.Deserialize<int>(metadata.State);
        PersonEntity? entity = await _context.Persons.FindAsync(personId);
        Person person = entity == null ? new Person() : entity.FromEntity();
        return person;
    }

    async protected override Task<byte[]> OnStoreSnapshotAsync(
                            EvDbStoredSnapshotData<Person> data, CancellationToken cancellation)
    {
        PersonEntity state = data.State;
        await _context.Persons.AddAsync(state);
        await _context.SaveChangesAsync();
        byte[] id = JsonSerializer.SerializeToUtf8Bytes(state.Id);
        return id;
    }
}

public abstract class EvDbTypedStorageStreamAdapter<TState> :
                        IEvDbStorageSnapshotAdapter<TState>
{
    private readonly IEvDbStorageSnapshotAdapter _adapter;

    public EvDbTypedStorageStreamAdapter(
        IEvDbStorageSnapshotAdapter adapter)
    {
        _adapter = adapter;
    }

    protected abstract Task<TState> OnGetSnapshotAsync(
                                        EvDbViewAddress viewAddress,
                                        EvDbStoredSnapshot metadata,
                                        CancellationToken cancellation);

    protected abstract Task<byte[]> OnStoreSnapshotAsync(
                                        EvDbStoredSnapshotData<TState> data,
                                        CancellationToken cancellation);

    async Task<EvDbStoredSnapshot<TState>> IEvDbStorageSnapshotAdapter<TState>.GetSnapshotAsync(
        EvDbViewAddress viewAddress,
        CancellationToken cancellation)
    {
        EvDbStoredSnapshot meta = await _adapter.GetSnapshotAsync(viewAddress, cancellation);
        TState state = await OnGetSnapshotAsync(viewAddress, meta, cancellation);
        return new EvDbStoredSnapshot<TState>(meta.Offset, state);
    }

    async Task IEvDbStorageSnapshotAdapter<TState>.StoreSnapshotAsync(
        EvDbStoredSnapshotData<TState> data,
        CancellationToken cancellation)
    {
        using var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        byte[] buffer = await OnStoreSnapshotAsync(data, cancellation);
        EvDbStoredSnapshotData snapshot = new(data, data.Offset, buffer);
        await _adapter.StoreSnapshotAsync(snapshot, cancellation);
    }
}
