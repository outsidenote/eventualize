// Ignore Spelling: Postgres

using EvDb.Core.Adapters;
using Npgsql;
using System.Data.Common;

namespace EvDb.Adapters.Store.Postgres;

public sealed class EvDbPostgresConnectionFactory : IEvDbConnectionFactory
{
    private readonly string _connectionString;

    public EvDbPostgresConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    string IEvDbConnectionFactory.ProviderType { get; } = "Postgres";

    DbConnection IEvDbConnectionFactory.CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
