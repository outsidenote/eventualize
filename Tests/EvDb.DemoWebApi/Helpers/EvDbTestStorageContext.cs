using EvDb.Core;

namespace EvDb.DemoWebApi;

public record EvDbTestStorageContext() : EvDbStorageContext(
    "master",
    "test",
    $"_eventualize_{Guid.NewGuid():N}",
    "dbo");