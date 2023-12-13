using Eventualize.Core;
using Eventualize.Core.Adapters.SqlStore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data.Common;

namespace CoreTests.StorageAdapterTests.SQLServerStorageAdapterTests;

public class SQLServerAdapterTestWorld : IDisposable, IAsyncDisposable
{
    public StorageContext ContextId { get; } = StorageContext.CreateUnique();
    public IStorageAdapter StorageAdapter { get; }
    public IStorageMigration StorageMigration { get; }
    public DbConnection Connection { get; }

    public SQLServerAdapterTestWorld(IConfigurationRoot configuration)
    {
        string connectionString = configuration.GetConnectionString("TestConnection") ?? throw new ArgumentNullException("TestConnection");
        Func<DbConnection> factory = () => new SqlConnection(connectionString);
        StorageAdapter = new SqlServerStorageAdapter(factory, ContextId);
        StorageMigration = new SqlServerStorageMigration(factory, ContextId);
        Connection = factory();
        Connection.Open(); // TODO: [bnaya 2023-12-12] model it better
    }


    void IDisposable.Dispose()
    {
        DisposeAsync(true).Wait();
        GC.SuppressFinalize(this);
    }

    protected async virtual Task DisposeAsync(bool disposed)
    {
        await Connection.DisposeAsync();
        await StorageAdapter.DisposeAsync();
        await StorageMigration.DisposeAsync();
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await DisposeAsync(true);
    }

    ~SQLServerAdapterTestWorld()
    {
        DisposeAsync(false).Wait();
    }
}