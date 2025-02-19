// Ignore Spelling: MongoDB

using EvDb.Core.Adapters;
using Npgsql;
using System.Data.Common;

namespace EvDb.Adapters.Store.MongoDB;

public sealed class EvDbMongoDBConnectionFactory : EvDbConnectionFactory
{
    private readonly string _connectionString;

    public EvDbMongoDBConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public override DbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }

    public override string ProviderType => "SqlServer";
}
