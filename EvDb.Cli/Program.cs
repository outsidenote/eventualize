using Cocona;
using Cocona.Builder;
using EvDb.Adapters.Store.SqlServer;
using EvDb.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;


var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

CoconaAppBuilder builder = CoconaApp.CreateBuilder();
builder.Logging.AddConsole();
builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.json", true, true)
            .AddJsonFile($"appsettings.{environmentName}.json", true, true);
var services = builder.Services;

var app = builder.Build();
await app.RunAsync(async (
        ILogger<Program> logger,
        [Option("action", ['a'],
                ValueName = "Enum",
                Description = "Action (`Create`, `Drop`)")]
        Operation operation,
        [Option("database", ['d'],
                Description = "Database type (`sql-server` | `posgres`, etc.)")]
        string db,
        [Option("name", ['n'],
                ValueName = "String",
                Description = "Database/Collection name")]
        EvDbDatabaseName name,
        [Option("connection-string", ['c'],
                Description = "Connection string")]
        string connectionString,
        [Option("environment", ['e'],
                Description = "Environment (Development, Production, QA, etc.)")]
        string? env = null,
        [Option("prefix", ['p'],
                ValueName = "String",
                Description = "Table prefix")]
        EvDbShardName? prefix = null,
        [Option("schema", ['s'],
                ValueName = "String",
                Description = "Schema of the tables")]
        EvDbSchemaName? schema = null,
        [Option("outbox", ['o'],
                Description = "outbox tables names (ALLOW MULTIPLE)")]
        EvDbShardName[]? outbox = null,
        [Option("features", ['f'],
                ValueName = "Enum",
                Description = "Storage features like: `stream`, `snapshot`, `outbox`, `all`")]
        StorageFeatures[]? features = null
        ) =>
{
    StorageFeatures storageFeatures = features == null || features.Length == 0
                                        ? StorageFeatures.All
                                        : StorageFeatures.None;
    foreach (var f in features ?? Array.Empty<StorageFeatures>())
    {
        storageFeatures |= f;
    }

    var context = new EvDbStorageContext(name, env, prefix, schema);

    EvDbShardName[] outboxNames = outbox ?? Array.Empty<EvDbShardName>();

    connectionString = Environment.GetEnvironmentVariable(connectionString) ?? connectionString;
    IEvDbStorageMigration migration = db switch
    {
        "sql-server" => SqlServerStorageMigrationFactory.Create(logger, connectionString, context, storageFeatures, outboxNames),
        //"posgres" => PostgresStorageMigrationFactory.Create(logger, connectionString, context, storageFeatures, outboxNames),
        _ => throw new NotImplementedException()
    };

    if (operation == Operation.Create)
    {
        logger.LogInformation("Creating...");
        await migration.CreateEnvironmentAsync();
    }
    else if (operation == Operation.Drop)
    {
        logger.LogInformation("Dropping...");
        await migration.DestroyEnvironmentAsync();
    }
    logger.LogInformation("Complete");
});