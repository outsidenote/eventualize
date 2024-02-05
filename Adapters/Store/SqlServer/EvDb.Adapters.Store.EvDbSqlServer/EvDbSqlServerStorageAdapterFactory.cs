using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Data.SqlClient;

namespace EvDb.Adapters.Store.SqlServer;

public static class EvDbSqlServerStorageAdapterFactory
{
    public static IEvDbStorageAdapter Create(
        ILogger logger,
        IEvDbConnectionFactory factory,
        EvDbStorageContext context)
    {
        IEvDbStorageAdapter result = new EvDbSqlServerStorageAdapter(
                    logger,
                    context,
                    factory);
        return result;
    }

    public static IEvDbStorageAdapter Create(
        ILogger logger,
        string connectionString,
        EvDbStorageContext context)
    {
        IEvDbConnectionFactory factory = new EvDbSqlConnectionFactory(connectionString);
        var result = Create(logger, factory, context);
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