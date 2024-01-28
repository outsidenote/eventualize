using Generator.Equals;
using System.Diagnostics;

namespace EvDb.Core;

// TODO: [bnaya 2021-12-27] make it a struct with casting to EvDbEvent
[Equatable]
[DebuggerDisplay("{EventType}: {Data}")]
public partial record EvDbStoredEvent(string EventType,
                    DateTime CapturedAt,
                    string CapturedBy,
                    string Data,
                    DateTime StoredAt,
                    EvDbStreamCursor StreamCursor)
                        : EvDbEvent(EventType, CapturedAt, CapturedBy, StreamCursor, Data)
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
}