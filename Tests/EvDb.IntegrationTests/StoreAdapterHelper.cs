﻿using EvDb.Adapters.Store.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EvDb.Core.Tests;

public static class StoreAdapterHelper
{
    public static IEvDbStorageAdapter CreateStoreAdapter(
        ILogger logger,
        StoreType storeType,
        EvDbTestStorageContext context)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

        string connectionKey = storeType switch
        {
            StoreType.SqlServer => "SqlServerConnection",
            StoreType.Posgres => "PosgresConnection",
            _ => throw new NotImplementedException()
        };


        string connectionString = configuration.GetConnectionString(connectionKey) ?? throw new ArgumentNullException(connectionKey);

        IEvDbStorageAdapter result = storeType switch
        {
            StoreType.SqlServer =>
                SqlServerStorageAdapterFactory.Create(logger, connectionString, context),
            //StoreType.Posgres => ,
            //    PosgresStorageAdapterFactory.Create(logger, connectionString, context),
            _ => throw new NotImplementedException()
        };
        return result;
    }

    public static IEvDbStorageMigration CreateStoreMigration(
        ILogger logger,
        StoreType storeType,
        EvDbTestStorageContext? context = null)
    {
        context = context ?? new EvDbTestStorageContext();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

        string connectionKey = storeType switch
        {
            StoreType.SqlServer => "SqlServerConnection",
            StoreType.Posgres => "PosgresConnection",
            _ => throw new NotImplementedException()
        };


        string connectionString = configuration.GetConnectionString(connectionKey) ?? throw new ArgumentNullException(connectionKey);

        IEvDbStorageMigration result = storeType switch
        {
            StoreType.SqlServer =>
                SqlServerStorageMigration.Create(logger, connectionString, context),
            //StoreType.Posgres => ,
            //    PosgresStorageAdapterFactory.Create(logger, connectionString, context),
            _ => throw new NotImplementedException()
        };
        return result;
    }

}
