using Generator.Equals;

namespace EvDb.Core;

// TODO: [bnaya 2021-12-27] make it a struct with casting to EvDbEvent
[Equatable]
public partial record EvDbStoredEvent(string EventType,
                    DateTime CapturedAt,
                    string CapturedBy,
                    string Data,
                    DateTime StoredAt,
                    EvDbStreamCursor StreamCursor)
                        : EvDbEvent(EventType, CapturedAt, CapturedBy, Data)
                            , IEvDbStoredEvent
{
    public EvDbStoredEvent(IEvDbEvent e, EvDbStreamCursor StreamCursor)
        : this(e.EventType, e.CapturedAt, e.CapturedBy, ((EvDbEvent)e).Data, DateTime.Now, StreamCursor) { }

    public EvDbStoredEvent(IEvDbEvent e, EvDbSnapshotCursor cursor)
        : this(e.EventType, e.CapturedAt, e.CapturedBy,
              ((EvDbEvent)e).Data, DateTime.Now,
              new EvDbStreamCursor(cursor))
    {
    }

    public IEvDbEvent PendingEvent { get; }
    public EvDbSnapshotCursor Cursor { get; }
}