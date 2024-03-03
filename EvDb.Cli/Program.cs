using EvDb.Adapters.Store.SqlServer;
//using EvDb.Adapters.Store.Posgres;
using EvDb.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

var logger = new LoggerFactory()
    .CreateLogger<Program>();

int storeIndex = Array.FindIndex(args, m => m == "--store");
int indexSonnectionString = Array.FindIndex(args, m => m == "--connection-string");
int indexnv = Array.FindIndex(args, m => m == "--environment" || m == "-e");
string? env = indexnv == -1
    ? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    : args[indexnv + 1];
string? connectionString = indexSonnectionString == -1
    ? Environment.GetEnvironmentVariable("EVDB_CONNECTION")
    : Environment.GetEnvironmentVariable(args[indexSonnectionString + 1]) ?? args[indexSonnectionString + 1];

if(string.IsNullOrEmpty(connectionString))
    Console.WriteLine("Connection string is missing!");

if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(env) || storeIndex == -1 || args.Length % 2 == 0 || args.Length < storeIndex + 2)
{
    var versionString = Assembly.GetEntryAssembly()?
                            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                            .InformationalVersion
                            .ToString();

    Console.WriteLine($"""
                EvDb v{versionString}  
                    Switches:
                      --connection-string: Environment variable name, if missing use `EVDB_CONNECTION`
                      --environment, -e: The environment (mandatory)
                      --store: one of [`sql-server` | `posgres] (mandatory)
                    Usage:
                      evdb --store <sql-server|posgres> -e <environment> --connection-string <environment variable key> <prefix>
                      evdb --store sql-server -e dev --connection-string MY_CONN <prefix>
                """
    );
    return;
}



var context = EvDbStorageContext.CreateWithEnvironment(args[^0], env);

IEvDbStorageMigration store = args[storeIndex + 1] switch
{
    "sql-server" => SqlServerStorageMigrationFactory.Create(logger, connectionString, context),
    //"posgres" => PostgresStorageMigrationFactory.Create(logger, connectionString, context),
    _ => throw new NotImplementedException()
};

await store.CreateEnvironmentAsync();