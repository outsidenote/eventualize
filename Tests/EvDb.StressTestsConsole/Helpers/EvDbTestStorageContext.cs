using EvDb.Core;

namespace EvDb.StressTests;

public record EvDbTestStorageContext(StoreType storeType) : EvDbStorageContext(
     storeType switch
     {
         StoreType.SqlServer => "master",
         StoreType.Posgres => "test_db",
         StoreType.MongoDB => "stress",
         _ => throw new NotSupportedException()
     },
    "test",
    $"stress_{DateTime.Now:HH_mm_ss}",
    storeType switch
    {
        StoreType.SqlServer => "dbo",
        StoreType.Posgres => "public",
        StoreType.MongoDB => "tests",
        _ => throw new NotSupportedException()
    });