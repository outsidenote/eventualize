// Ignore Spelling: Postgres

using EvDb.Core.Adapters;
using System.Data.Common;
using Npgsql;

namespace EvDb.Adapters.Store.Postgres;

public sealed class EvDbPostgresConnectionFactory : EvDbConnectionFactory
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

    public override string ProviderType => "SqlServer";
}
