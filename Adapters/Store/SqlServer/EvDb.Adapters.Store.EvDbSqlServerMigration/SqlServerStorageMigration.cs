using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Data.SqlClient;

namespace EvDb.Adapters.Store.SqlServer;

public static class SqlServerStorageMigration
{
    public static IEvDbStorageMigration Create(
        ILogger logger,
        IEvDbConnectionFactory factory,
        EvDbStorageContext context)
    {
        IEvDbStorageMigration result =
            EvDbRelationalStorageMigration.Create(
                    logger,
                    QueryTemplatesFactory.Create(context),
                    factory);
        return result;
    }

    public static IEvDbStorageMigration Create(
        ILogger logger,
        string connectionString,
        EvDbStorageContext context)
    {
        IEvDbConnectionFactory factory = new EvDbSqlConnectionFactory(connectionString);

        IEvDbStorageMigration result =
            EvDbRelationalStorageMigration.Create(
                    logger,
                    QueryTemplatesFactory.Create(context),
                    factory);
        return result;
    }
}

