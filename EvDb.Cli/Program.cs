﻿using Cocona;
using Cocona.Builder;
using EvDb.Adapters.Store.Postgres;
using EvDb.Adapters.Store.SqlServer;
using EvDb.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

CoconaAppBuilder builder = CoconaApp.CreateBuilder();
builder.Logging.AddConsole();
builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.json", true, true)
            .AddJsonFile($"appsettings.{environmentName}.json", true, true);

var app = builder.Build();

await app.RunAsync(async (
        ILogger<Program> logger,
        [Option("action", ['a'],
                ValueName = "Enum",
                Description = "Action (`CreateParser`, `Drop`)")]
        Operation operation,
        [Option("database", ['d'],
                Description = "Database type (`sql-server` | `postgres`, etc.)")]
        string db,
        [Option("name", ['n'],
                ValueName = "String",
                Description = "Database/Collection name")]
        EvDbDatabaseName name,
        [Option("connection-string", ['c'],
                Description = "Connection string")]
        string? connectionString = null,
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
        StorageFeatures[]? features = null,
        [Option("dry-run",
                Description = "Only writes to the console")]
        bool dryRun = false,
        [Argument("path", Description = "Path for a script file won't affect the DB when exists)")]
        string? path = null
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

    IEvDbStorageAdmin admin = db switch
    {
        "sql-server" => SqlServerStorageAdminFactory.Create(logger, connectionString!, context, storageFeatures, outboxNames),
        "postgres" => PostgresStorageAdminFactory.Create(logger, connectionString!, context, storageFeatures, outboxNames),
        _ => throw new NotImplementedException()
    };

    if (!dryRun && string.IsNullOrWhiteSpace(path))
    {
        if (operation == Operation.Create)
        {
            logger.LogInformation("Creating...");
            await admin.CreateEnvironmentAsync();
        }
        else if (operation == Operation.Drop)
        {
            logger.LogInformation("Dropping...");
            await admin.DestroyEnvironmentAsync();
        }
    }
    else
    {
        EvDbMigrationQueryTemplates scripts = admin.Scripts;

        if (operation == Operation.Create)
        {
            logger.LogInformation(string.Join("""

                    """, scripts.CreateEnvironment));
            if (!dryRun)
            {
                logger.LogInformation("Saving `create` script into [{Path}]", path);
                await File.WriteAllLinesAsync(path!, scripts.CreateEnvironment);
            }
        }
        else if (operation == Operation.Drop)
        {
            logger.LogInformation(scripts.DestroyEnvironment);
            if (!dryRun)
            {
                logger.LogInformation("Saving `drop` script into [{Path}]", path);
                await File.WriteAllTextAsync(path!, scripts.DestroyEnvironment);
            }
        }
    }
    logger.LogInformation("Complete");
});