namespace Eventualize.Core;

public interface IEventualizeStoredEvent : IEventualizeEvent
{
    DateTime StoredAt { get; }
    EventualizeStreamCursor StreamCursor { get; }
}
