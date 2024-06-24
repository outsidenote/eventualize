using EvDb.Core;

namespace EvDb.StressTestsWebApi;

public class EvDbTestStorageContext : EvDbStorageContext
{

    public EvDbTestStorageContext() : base($"_eventualize_{Guid.NewGuid():N}", "test")
    {

    }
}