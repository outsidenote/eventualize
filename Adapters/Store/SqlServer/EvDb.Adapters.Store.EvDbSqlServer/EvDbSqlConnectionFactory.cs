using EvDb.Core.Adapters;
using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace EvDb.Adapters.Store.SqlServer;

public sealed class EvDbSqlConnectionFactory : IEvDbConnectionFactory
{
    private readonly string _connectionString;

    public EvDbSqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    string IEvDbConnectionFactory.ProviderType { get; } = "SqlServer";

    DbConnection IEvDbConnectionFactory.CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
