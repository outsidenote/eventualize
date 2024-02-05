using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data.Common;

namespace EvDb.Adapters.Store.Postgres;

public static class EvDbPostgresStorageAdapterFactory
{
    public static IEvDbStorageAdapter Create(
        ILogger logger,
        IEvDbConnectionFactory factory,
        EvDbStorageContext context)
    {
        IEvDbStorageAdapter result =
            new EvDbPostgresStorageAdapter(
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
        IEvDbConnectionFactory factory = new EvDbPostgresConnectionFactory(connectionString);
        var result = Create(logger, factory, context);
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