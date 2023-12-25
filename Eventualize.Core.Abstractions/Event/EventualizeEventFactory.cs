namespace Eventualize.Core;

public static class EventualizeEventFactory
{
    public static EventualizeEvent<T> Create<T>(string eventType,
                                       DateTime capturedAt,
                                       string capturedBy,
                                       T data) => new EventualizeEvent<T>(
                                                    eventType,
                                                    capturedAt,
                                                    capturedBy,
                                                    data); 
}
