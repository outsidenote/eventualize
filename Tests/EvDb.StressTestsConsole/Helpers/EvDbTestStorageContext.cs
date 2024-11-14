using EvDb.Core;

namespace EvDb.StressTests;

public record EvDbTestStorageContext() : EvDbStorageContext(
    "master",
    "test",
    $"stress_{DateTime.Now:HH_mm_ss}",
    "dbo");