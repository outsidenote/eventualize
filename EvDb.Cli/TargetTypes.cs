[Flags]
public enum TargetTypes
{
    None,
    Stream = 1,
    Snapshot = Stream * 2,
    Outbox = Snapshot * 2,
    All = Stream | Snapshot | Outbox
}
