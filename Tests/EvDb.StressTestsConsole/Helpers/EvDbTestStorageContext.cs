using EvDb.Core;

namespace EvDb.StressTests;

public record EvDbTestStorageContext() : EvDbStorageContext(
    "master",
    "test",
    $"_eventualize_{Guid.NewGuid():N}",
    "dbo");