//// Ignore Spelling: Sql

//namespace EvDb.Core.Tests;

//using Dapper;
//using System.Threading.Tasks;
//using Xunit.Abstractions;

//public class PostgresEfSimpleTests : StreamEfBaseTests
//{
//    private const string CREATE_SCRIPT =
//        """
//        -- Create People table
//        CREATE TABLE People (
//            Id SERIAL PRIMARY KEY,
//            Name TEXT NOT NULL,
//            Birthday DATE NULL,
//            Country VARCHAR(255) NULL,
//            City VARCHAR(255) NULL,
//            Street VARCHAR(255) NULL
//        );

//        -- Create People2 table
//        CREATE TABLE People2 (
//            Id SERIAL PRIMARY KEY,
//            Name TEXT NOT NULL,
//            Birthday DATE NULL,
//            Country VARCHAR(255) NULL,
//            City VARCHAR(255) NULL,
//            Street VARCHAR(255) NULL
//        );

//        -- Create Emails table
//        CREATE TABLE Emails (
//            Id UUID PRIMARY KEY,
//            Value VARCHAR(255) NOT NULL,
//            PersonId INT NOT NULL,
//            Domain VARCHAR(255) NOT NULL,
//            Category VARCHAR(255) NOT NULL,
//            CONSTRAINT FK_Emails_People FOREIGN KEY (PersonId) REFERENCES People (Id) ON DELETE CASCADE
//        );

//        -- Create Emails2 table
//        CREATE TABLE Emails2 (
//            Id UUID PRIMARY KEY,
//            Value VARCHAR(255) NOT NULL,
//            PersonId INT NOT NULL,
//            Domain VARCHAR(255) NOT NULL,
//            Category VARCHAR(255) NOT NULL,
//            CONSTRAINT FK_Emails_People2 FOREIGN KEY (PersonId) REFERENCES People2 (Id) ON DELETE CASCADE
//        );

//        -- Create Indices
//        CREATE UNIQUE INDEX IX_People_Id ON People (Id);
//        CREATE UNIQUE INDEX IX_Email_Id ON Emails (Id);
//        CREATE INDEX IX_Emails_PersonId ON Emails (PersonId);

//        CREATE UNIQUE INDEX IX_People2_Id ON People2 (Id);
//        CREATE UNIQUE INDEX IX_Email2_Id ON Emails2 (Id);
//        CREATE INDEX IX_Emails_Person2Id ON Emails2 (PersonId);
//        """;

//    private const string DROP_SCRIPT =
//        """
//        -- Drop Table Scripts
//        DROP TABLE IF EXISTS Emails;
//        DROP TABLE IF EXISTS People;
//        DROP TABLE IF EXISTS Emails2;
//        DROP TABLE IF EXISTS People2;DROP TABLE IF EXISTS Emails CASCADE;
//        DROP TABLE IF EXISTS People CASCADE;
//        DROP TABLE IF EXISTS Emails2 CASCADE;
//        DROP TABLE IF EXISTS People2 CASCADE;
//        """;


//    public PostgresEfSimpleTests(ITestOutputHelper output) :
//        base(output, StoreType.Postgres)
//    {
//    }

//    public override async Task InitializeAsync()
//    {
//        await _connection.ExecuteAsync(DROP_SCRIPT);
//        await _connection.ExecuteAsync(CREATE_SCRIPT);
//        await base.InitializeAsync();
//    }

//    public async override Task DisposeAsync()
//    {
//        await _connection.ExecuteAsync(DROP_SCRIPT);
//        await base.DisposeAsync();
//    }
//}