using System.Diagnostics;

namespace EvDb.Core;

[DebuggerDisplay("{StreamId}, {Kind}, {EventType}, {Sequence}")]
public readonly record struct StoreEventsParameter(
                    string Domain,
                    string Partition,
                    string StreamId,
                    long Offset,
                    string EventType,
                    string Payload,
                    string CapturedBy,
                    DateTimeOffset CapturedAt)
{
    public StoreEventsParameter(EvDbEvent e)
            : this(
                e.StreamCursor.Domain,
                e.StreamCursor.Partition,
                e.StreamCursor.StreamId,
                e.StreamCursor.Offset,
                e.EventType,
                e.Payload,
                e.CapturedBy, e.CapturedAt)
    { 
    }

    public static implicit operator StoreEventsParameter(EvDbEvent e) => new StoreEventsParameter(e);
}