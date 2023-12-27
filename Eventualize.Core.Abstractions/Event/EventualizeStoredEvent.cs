using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventualize.Core;
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
                            , IEventualizeStoredEvent
{
    public EventualizeStoredEvent(EventualizeEvent e, EventualizeStreamCursor StreamCursor)
        : this(e.EventType, e.CapturedAt, e.CapturedBy, e.Data, DateTime.Now, StreamCursor) { }
}