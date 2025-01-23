using EvDb.Core.Adapters;
using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace EvDb.Adapters.Store.SqlServer;

public sealed class EvDbSqlConnectionFactory : EvDbConnectionFactory
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

    public override string ProviderType => "SqlServer";
}
