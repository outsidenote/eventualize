// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using Dapper;
using EvDb.Core.Adapters;
using EvDb.Scenes;
using EvDb.UnitTests;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit.Abstractions;
using static EvDb.Adapters.Store.SqlServer.EvDbSqlServerStorageAdapterFactory;

public class SqlServerEfSimpleTests : StreamEfBaseTests
{
    private const string CREATE_SCRIPT =
        """
        CREATE TABLE People (
            Id INT NOT NULL PRIMARY KEY,
            Name NVARCHAR(MAX) NOT NULL,
            Birthday DATE NULL,
            Country NVARCHAR(255) NULL,
            City NVARCHAR(255) NULL,
            Street NVARCHAR(255) NULL
        );

        CREATE TABLE Emails (
            Value NVARCHAR(255) NOT NULL PRIMARY KEY,
            PersonId INT NOT NULL,
            Domain NVARCHAR(255) NOT NULL,
            Category NVARCHAR(255) NOT NULL,
            CONSTRAINT FK_Emails_People FOREIGN KEY (PersonId) REFERENCES People (Id) ON DELETE CASCADE
        );

        -- Create Indices
        CREATE UNIQUE INDEX IX_People_Id ON People (Id);
        CREATE INDEX IX_Emails_PersonId ON Emails (PersonId);
        """;

    private const string DROP_SCRIPT =
        """
        -- Drop Table Scripts
        DROP TABLE IF EXISTS Emails;
        DROP TABLE IF EXISTS People;
        """;

    public SqlServerEfSimpleTests(ITestOutputHelper output) :
        base(output, StoreType.SqlServer)
    {
    }

    public override async Task InitializeAsync()
    {
        await _connection.ExecuteAsync(DROP_SCRIPT);
        await _connection.ExecuteAsync(CREATE_SCRIPT);
        await base.InitializeAsync();
    }

    public async override Task DisposeAsync()
    {
        await _connection.ExecuteAsync(DROP_SCRIPT);
        await base.DisposeAsync();
    }
}