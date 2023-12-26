using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventualize.Core.Abstractions.Stream;
using Generator.Equals;

namespace Eventualize.Core;

public partial record EventualizeStoredEvent(string EventType,
                    DateTime CapturedAt,
                    string CapturedBy,
                    string Data,
                    DateTime StoredAt,
                    EventualizeStreamCursor StreamCursor)
                    : EventualizeEvent(EventType, CapturedAt, CapturedBy, Data), IEventualizeStoredEvent
{
    public EventualizeStoredEvent(EventualizeEvent e, EventualizeStreamCursor StreamCursor)
        : this(e.EventType, e.CapturedAt, e.CapturedBy, e.Data, DateTime.Now, StreamCursor) { }
}