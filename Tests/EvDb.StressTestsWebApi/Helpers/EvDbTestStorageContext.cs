using EvDb.Core;

namespace EvDb.StressTestsWebApi;

public record EvDbTestStorageContext() : EvDbStorageContext(
    "master",
    "test",
    $"_eventualize_{Guid.NewGuid():N}",
    "dbo");