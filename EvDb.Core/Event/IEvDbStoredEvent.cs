namespace EvDb.Core;

public interface IEvDbStoredEvent : IEvDbEvent
{
    DateTime StoredAt { get; }
    EvDbStreamCursor StreamCursor { get; }
}
