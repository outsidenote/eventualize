using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data.Common;

namespace EvDb.Adapters.Store.Postgres;

public static class PostgresStorageMigration
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
        IEvDbConnectionFactory factory = new EvDbPostgresConnectionFactory(connectionString);

        IEvDbStorageMigration result =
            EvDbRelationalStorageMigration.Create(
                    logger,
                    QueryTemplatesFactory.Create(context),
                    factory);
        return result;

    }

    #region class EvDbSqlConnectionFactory : EvDbConnectionFactory

    private sealed class EvDbPostgresConnectionFactory : EvDbConnectionFactory
    {
        private readonly string _connectionString;

        public EvDbPostgresConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public override DbConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }

    #endregion // class EvDbSqlConnectionFactory : EvDbConnectionFactory
}