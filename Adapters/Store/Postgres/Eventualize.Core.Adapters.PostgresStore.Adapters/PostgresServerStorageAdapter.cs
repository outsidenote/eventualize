using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data.Common;

namespace Eventualize.Core.Adapters.SqlStore;

public static class PostgresStorageAdapter
{
    public static IEventualizeStorageAdapter Create(
        ILogger logger,
        IEventualizeConnectionFactory factory,
        EventualizeStorageContext context)
    {
        IEventualizeStorageAdapter result =
            EventualizeRelationalStorageAdapter.Create(
                    logger,
                    QueryTemplatesFactory.Create(context),
                    factory);
        return result;
    }

    public static IEventualizeStorageAdapter Create(
        ILogger logger,
        string connectionString,
        EventualizeStorageContext context)
    {
        IEventualizeConnectionFactory factory = new EventualizePostgresConnectionFactory(connectionString);

        IEventualizeStorageAdapter result =
            EventualizeRelationalStorageAdapter.Create(
                    logger,
                    QueryTemplatesFactory.Create(context),
                    factory);
        return result;

    }

    #region class EventualizeSqlConnectionFactory : EventualizeConnectionFactory

    private sealed class EventualizePostgresConnectionFactory : EventualizeConnectionFactory
    {
        private readonly string _connectionString;

        public EventualizePostgresConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public override DbConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }

    #endregion // class EventualizeSqlConnectionFactory : EventualizeConnectionFactory
}