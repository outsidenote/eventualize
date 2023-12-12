// Ignore Spelling: Sql

using Microsoft.Data.SqlClient;

namespace Eventualize.Core.Adapters.SqlStore;


// TODO: [bnaya 2023-12-10] base it on a shared logic (ADO.NET factory, Db...)
// TODO: [bnaya 2023-12-11] init from configuration
[Obsolete("deprecated")]
public static class SqlServerTestConnectionFactory
{
    public static SqlConnection CreateConnection()
    {
        SqlConnectionStringBuilder connectionBuilder = new SqlConnectionStringBuilder
        {
            DataSource = "localhost",
            UserID = "sa",
            Password = "MasadNetunim12!@",
            InitialCatalog = "master",
            TrustServerCertificate = true,
            MultipleActiveResultSets = true
        };

        var conn = new SqlConnection(connectionBuilder.ConnectionString);
        return conn;
    }
}