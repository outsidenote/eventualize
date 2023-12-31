using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Data.SqlClient;

namespace EvDb.Adapters.Store.SqlServer;

public static class SqlServerStorageAdapterFactory
{
    public static IEvDbStorageAdapter Create(
        ILogger logger,
        IEvDbConnectionFactory factory,
        EvDbStorageContext context)
    {
        IEvDbStorageAdapter result =
            EvDbRelationalStorageAdapter.Create(
                    logger,
                    QueryTemplatesFactory.Create(context),
                    factory);
        return result;
    }

    public static IEvDbStorageAdapter Create(
        ILogger logger,
        string connectionString,
        EvDbStorageContext context)
    {
        IEvDbConnectionFactory factory = new EvDbSqlConnectionFactory(connectionString);

        IEvDbStorageAdapter result =
            EvDbRelationalStorageAdapter.Create(
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