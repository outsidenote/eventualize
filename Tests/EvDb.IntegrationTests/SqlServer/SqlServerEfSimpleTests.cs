// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using Dapper;
using System.Threading.Tasks;
using Xunit.Abstractions;

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
        CREATE TABLE People2 (
            Id INT NOT NULL PRIMARY KEY,
            Name NVARCHAR(MAX) NOT NULL,
            Birthday DATE NULL,
            Country NVARCHAR(255) NULL,
            City NVARCHAR(255) NULL,
            Street NVARCHAR(255) NULL
        );

        CREATE TABLE Emails (
            Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
            Value NVARCHAR(255) NOT NULL,
            PersonId INT NOT NULL,
            Domain NVARCHAR(255) NOT NULL,
            Category NVARCHAR(255) NOT NULL,
            CONSTRAINT FK_Emails_People FOREIGN KEY (PersonId) REFERENCES People (Id) ON DELETE CASCADE
        );
        CREATE TABLE Emails2 (
            Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
            Value NVARCHAR(255) NOT NULL,
            PersonId INT NOT NULL,
            Domain NVARCHAR(255) NOT NULL,
            Category NVARCHAR(255) NOT NULL,
            CONSTRAINT FK_Emails_People2 FOREIGN KEY (PersonId) REFERENCES People2 (Id) ON DELETE CASCADE
        );

        -- Create Indices
        CREATE UNIQUE INDEX IX_People_Id ON People (Id);
        CREATE UNIQUE INDEX IX_Email_Id ON Emails (Id);
        CREATE INDEX IX_Emails_PersonId ON Emails (PersonId);

        CREATE UNIQUE INDEX IX_People2_Id ON People2 (Id);
        CREATE UNIQUE INDEX IX_Email2_Id ON Emails2 (Id);
        CREATE INDEX IX_Emails_Person2Id ON Emails2 (PersonId);
        """;

    private const string DROP_SCRIPT =
        """
        -- Drop Table Scripts
        DROP TABLE IF EXISTS Emails;
        DROP TABLE IF EXISTS People;
        DROP TABLE IF EXISTS Emails2;
        DROP TABLE IF EXISTS People2;
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