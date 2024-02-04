namespace EvDb.Core.Tests;

public class EvDbTestStorageContext : EvDbStorageContext
{

    public EvDbTestStorageContext() : base($"_eventualize_{Guid.NewGuid():N}", "test")
    {

    }
}
