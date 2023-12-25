using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventualize.Core.Abstractions.Stream;

namespace Eventualize.Core;

public record EventualizeStoredEvent<T>(string EventType,
                    DateTime CapturedAt,
                    string CapturedBy,
                    T Data,
                    DateTime StoredAt,
                    EventualizeStreamCursor StreamCursor)
                    : EventualizeEvent<T>(EventType, CapturedAt, CapturedBy, Data), IEventualizeStoredEvent
{
    public EventualizeStoredEvent(EventualizeEvent<T> e, EventualizeStreamCursor StreamCursor)
        : this(e.EventType, e.CapturedAt, e.CapturedBy, e.Data, DateTime.Now, StreamCursor) { }
}