namespace Eventualize.Core;

// TODO: [bnaya 2023-12-10] Should be an interface injected via DI (prod should be null or add environment to it)
public class StorageAdapterContextId
{
    public string ContextId { get; private set; }

    public StorageAdapterContextId()
    {
        ContextId = "live";
    }

    public StorageAdapterContextId(Guid guid)
    {
        ContextId = guid.ToString("N");
    }

    // TODO: [bnaya 2023-12-11] remove the testcontextid_
    public override string ToString()
    {
        if (ContextId == "live")
            return "";
        var formatted = ContextId.Replace("-", "_") + "_";
        return $"testcontextid_{formatted}";
    }
}