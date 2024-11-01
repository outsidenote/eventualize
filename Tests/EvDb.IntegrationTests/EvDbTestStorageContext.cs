namespace EvDb.Core.Tests;

public record EvDbTestStorageContext(): EvDbStorageContext(
    "master",
    "test",
    $"_eventualize_{Guid.NewGuid():N}", 
    "dbo");
