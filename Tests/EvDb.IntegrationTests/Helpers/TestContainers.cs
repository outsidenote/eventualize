// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

public class TestContainers 
{
    public IContainer Container { get; private set; }

    public async Task StartAllAsync(StoreType storeType)
    {
        if (storeType == StoreType.SqlServer)
        {
            Container = new ContainerBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithEnvironment("ACCEPT_EULA", "Y")
                .WithEnvironment("SA_PASSWORD", "MasadNetunim12!@")
                .WithPortBinding(1433, 1433)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
                .Build();
        }
        else if (storeType == StoreType.Postgres)
        {
            Container = new ContainerBuilder()
                .WithImage("postgres:latest")
                .WithEnvironment("POSTGRES_USER", "test_user")
                .WithEnvironment("POSTGRES_PASSWORD", "MasadNetunim12!@")
                .WithEnvironment("POSTGRES_DB", "test_db")
                .WithPortBinding(5432, 5432)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
                .Build();
        }
        else if (storeType == StoreType.MongoDB)
        {

            Container = new ContainerBuilder()
                .WithImage("mongo:8.0")
                .WithCommand("mongod", "--replSet", "rs0", "--bind_ip_all")
                .WithPortBinding(27017, 27017)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(27017))
                .Build();
        }

        await Container.StartAsync();

        if (storeType == StoreType.MongoDB)
        {
            // Initialize replica set for Mongo
            var mongoInit = new ContainerBuilder()
                .WithImage("mongo:8.0")
                .WithCommand("sh", "-c", "sleep 5 && mongosh --host localhost:27017 --eval \"rs.initiate({_id: 'rs0', members: [{ _id: 0, host: 'localhost:27017' }]})\"")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(""))
                .Build();

            await mongoInit.StartAsync();
            await mongoInit.DisposeAsync();
        }
    }

    public async Task StopAllAsync()
    {
        await Container.DisposeAsync();
    }
}