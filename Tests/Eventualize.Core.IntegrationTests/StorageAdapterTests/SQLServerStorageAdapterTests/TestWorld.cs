using Castle.Core.Configuration;
using Eventualize.Core;
using Eventualize.Core.Adapters;
using Eventualize.Core.Adapters.PostgresStore;
using Eventualize.Core.Adapters.SqlStore;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using MySql.Data.MySqlClient;
using Npgsql;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using Xunit.Abstractions;

namespace CoreTests.StorageAdapterTests.SQLServerStorageAdapterTests;

public class TestWorld : IDisposable, IAsyncDisposable
{
    public EventualizeStorageContext ContextId { get; } = EventualizeStorageContext.CreateUnique();
    public IEventualizeStorageAdapter StorageAdapter { get; private set; }
    public IEventualizeStorageMigration StorageMigration { get; private set; }
    public DbConnection Connection { get; private set; }
    public TypeOfDb TypeOfDb { get; }

    public readonly ILogger _logger = A.Fake<ILogger>();
    private readonly IEventualizeConnectionFactory _connectionFactory = A.Fake<IEventualizeConnectionFactory>();
    protected readonly ITestOutputHelper _testLogger;
    private readonly IConfigurationRoot _configuration;


    public static async Task<TestWorld> CreateAsync(TypeOfDb dbType, IConfigurationRoot configuration, ITestOutputHelper testLogger)
    {
        var world = new TestWorld(configuration, testLogger, dbType);
        await world.InitAsync(dbType);
        await world.StorageMigration.CreateTestEnvironmentAsync();
        return world;
    }

    private TestWorld(
            IConfigurationRoot configuration,
            ITestOutputHelper testLogger, 
            TypeOfDb typeOfDb)
    {
        _testLogger = testLogger;
        _configuration = configuration;
        TypeOfDb = typeOfDb;
    }

    private async Task InitAsync(TypeOfDb dbType)
    {
        string connKey = dbType switch
        {
            TypeOfDb.SqlServer => "SqlServerConnection",
            TypeOfDb.Postgress => "PostgresConnection",
            TypeOfDb.MySql => "MySqlConnection",
            _ => throw new NotImplementedException()
        };
        string connectionString = _configuration.GetConnectionString(connKey) ?? throw new ArgumentNullException(connKey);

        A.CallTo(() => _connectionFactory.CreateConnection())
            .ReturnsLazily(() =>
            {
                DbConnection conn = dbType switch
                {
                    TypeOfDb.SqlServer => new SqlConnection(connectionString),
                    TypeOfDb.Postgress => new NpgsqlConnection(connectionString),
                    TypeOfDb.MySql => new MySqlConnection(connectionString),
                    _ => throw new NotImplementedException()
                };

                // TBD: [bnaya 2023-12-20] https://www.carlrippon.com/instrumenting-dapper-queries-in-asp-net-core/
                //conn.StatisticsEnabled = true;
                return conn;
            });
        StorageAdapter = dbType switch
        {
            TypeOfDb.SqlServer => SqlServerStorageAdapter.Create(_logger, _connectionFactory, ContextId),
            TypeOfDb.Postgress => PostgresStorageAdapter.Create(_logger, _connectionFactory, ContextId),
            //TypeOfDb.MySql => ,
            _ => throw new NotImplementedException()
        };
        StorageMigration = dbType switch
        {
            TypeOfDb.SqlServer => SqlServerStorageMigration.Create(_logger, _connectionFactory, ContextId),
            TypeOfDb.Postgress => PostgresStorageMigration.Create(_logger, _connectionFactory, ContextId),
            //TypeOfDb.MySql => ,
            _ => throw new NotImplementedException()
        };

        Connection = _connectionFactory.CreateConnection();
        await Connection.OpenAsync(); // TODO: [bnaya 2023-12-12] model it better
    }

    void IDisposable.Dispose()
    {
        DisposeAsync(true).Wait();
        GC.SuppressFinalize(this);
    }

    protected async virtual Task DisposeAsync(bool disposed)
    {
        await StorageMigration.DestroyTestEnvironmentAsync();
        await Connection.DisposeAsync();
        await StorageAdapter.DisposeAsync();
        await StorageMigration.DisposeAsync();
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await DisposeAsync(true);
    }

    ~TestWorld()
    {
        DisposeAsync(false).Wait();
    }
}