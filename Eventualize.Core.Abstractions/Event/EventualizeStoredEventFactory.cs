namespace Eventualize.Core;

public static class EventualizeStoredEventFactory
{
    public static EventualizeStoredEvent<T> Create<T>(string eventType,
                                        DateTime capturedAt,
                                        string capturedBy,
                                        T data,
                                        DateTime storedAt,
                                        EventualizeStreamCursor streamCursor) => new EventualizeStoredEvent<T>(
                                                    eventType,
                                                    capturedAt,
                                                    capturedBy,
                                                    data,
                                                    storedAt,
                                                    streamCursor);

    public static EventualizeStoredEvent<T> Create<T>(EventualizeEvent<T> e, EventualizeStreamCursor streamCursor) => new EventualizeStoredEvent<T>(e.EventType,
                                    e.CapturedAt,
                                    e.CapturedBy,
                                    e.Data,
                                    DateTime.UtcNow,
                                    streamCursor); 
}
