namespace EvDb.Core;

[Flags]
public enum StorageFeatures
{
    None,
    Stream = 1,
    Snapshot = Stream * 2,
    Outbox = Snapshot * 2,
    //GetMessages = Outbox * 2,
    StreamAndOutbox = Stream | Outbox,
    All = Stream | Snapshot | Outbox // | GetMessages
}
