using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Data.SqlClient;

namespace Eventualize.Core.Adapters.SqlStore;

public static class SqlServerStorageAdapterFactory
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
        IEventualizeConnectionFactory factory = new EventualizeSqlConnectionFactory(connectionString);

        IEventualizeStorageAdapter result =
            EventualizeRelationalStorageAdapter.Create(
                    logger,
                    QueryTemplatesFactory.Create(context),
                    factory);
        return result;

    }

    #region class EventualizeSqlConnectionFactory : EventualizeConnectionFactory

    private sealed class EventualizeSqlConnectionFactory : EventualizeConnectionFactory
    {
        private readonly string _connectionString;

        public EventualizeSqlConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public override DbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }

    #endregion // class EventualizeSqlConnectionFactory : EventualizeConnectionFactory
}