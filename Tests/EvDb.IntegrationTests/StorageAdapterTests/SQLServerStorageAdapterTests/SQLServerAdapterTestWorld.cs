using EvDb.Adapters.Store.SqlServer;
using EvDb.Core;
using EvDb.Core.Adapters;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Data.SqlClient;
using Xunit.Abstractions;

namespace CoreTests.StorageAdapterTests.SQLServerStorageAdapterTests;

public class SQLServerAdapterTestWorld : IDisposable, IAsyncDisposable
{
    public EvDbStorageContext ContextId { get; } = EvDbStorageContext.CreateUnique();
    public IEvDbStorageAdapter StorageAdapter { get; }
    public IEvDbStorageMigration StorageMigration { get; }
    public DbConnection Connection { get; }
    public readonly ILogger _logger = A.Fake<ILogger>();
    private readonly IEvDbConnectionFactory _connectionFactory = A.Fake<IEvDbConnectionFactory>();
    private readonly ITestOutputHelper _testLogger;

    public SQLServerAdapterTestWorld(IConfigurationRoot configuration, ITestOutputHelper testLogger)
    {
        string connectionString = configuration.GetConnectionString("SqlServerConnection") ?? throw new ArgumentNullException("SqlServerConnection");

        A.CallTo(() => _connectionFactory.CreateConnection())
            .ReturnsLazily(() =>
            {
                var conn = new SqlConnection(connectionString);
                // TBD: [bnaya 2023-12-20] https://www.carlrippon.com/instrumenting-dapper-queries-in-asp-net-core/
                //conn.StatisticsEnabled = true;
                return conn;
            });
        StorageAdapter = SqlServerStorageAdapterFactory.Create(_logger, _connectionFactory, ContextId);
        StorageMigration = SqlServerStorageMigration.Create(_logger, _connectionFactory, ContextId);
        Connection = _connectionFactory.CreateConnection();
        Connection.Open(); // TODO: [bnaya 2023-12-12] model it better
        _testLogger = testLogger;
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