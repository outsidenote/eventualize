namespace EvDb.Core.Tests;

public record EvDbTestStorageContext(EvDbSchemaName schema, EvDbDatabaseName db) : EvDbStorageContext(
    db,
    "test",
    $"_eventualize_{Guid.NewGuid():N}",
    schema);
