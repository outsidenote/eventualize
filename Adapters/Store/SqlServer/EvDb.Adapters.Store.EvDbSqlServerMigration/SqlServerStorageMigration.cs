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

    #region class EvDbSqlConnectionFactory : EvDbConnectionFactory

    private sealed class EvDbSqlConnectionFactory : EvDbConnectionFactory
    {
        private readonly string _connectionString;

        public EvDbSqlConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public override DbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }

    #endregion // class EvDbSqlConnectionFactory : EvDbConnectionFactory
}