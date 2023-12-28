using Generator.Equals;

namespace Eventualize.Core;

// TODO: [bnaya 2021-12-27] make it a struct with casting to EventualizeEvent
[Equatable]
public partial record EventualizeStoredEvent(string EventType,
                    DateTime CapturedAt,
                    string CapturedBy,
                    string Data,
                    DateTime StoredAt,
                    EventualizeStreamCursor StreamCursor)
                        : EventualizeEvent(EventType, CapturedAt, CapturedBy, Data)
                            ,IEventualizeStoredEvent
{
    public EventualizeStoredEvent(IEventualizeEvent e, EventualizeStreamCursor StreamCursor)
        : this(e.EventType, e.CapturedAt, e.CapturedBy, ((EventualizeEvent)e).Data, DateTime.Now, StreamCursor) { }

    public EventualizeStoredEvent(IEventualizeEvent e, EventualizeSnapshotCursor cursor)
        : this(e.EventType, e.CapturedAt, e.CapturedBy, 
              ((EventualizeEvent)e).Data, DateTime.Now, 
              new EventualizeStreamCursor(cursor))
    {
    }

    public IEventualizeEvent PendingEvent { get; }
    public EventualizeSnapshotCursor Cursor { get; }
}