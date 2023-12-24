using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventualize.Core.Abstractions.Stream;

namespace Eventualize.Core;

public record EventualizeStoredEvent(string EventType,
                    DateTime CapturedAt,
                    string CapturedBy,
                    string JsonData,
                    DateTime StoredAt,
                    EventualizeStreamUri StreamUri,
                    long Offset)
                    : EventualizeEvent(EventType, CapturedAt, CapturedBy, JsonData)
{
    public EventualizeStoredEvent(EventualizeEvent e, EventualizeStreamUri streamUri, long offset)
        : this(e.EventType, e.CapturedAt, e.CapturedBy, e.JsonData, DateTime.Now, streamUri, offset) { }
}