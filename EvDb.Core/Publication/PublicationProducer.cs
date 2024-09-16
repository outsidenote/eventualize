// Ignore Spelling: OutboxHandler Outbox

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EvDb.Core.Internals;

public abstract class OutboxHandlerBase
{
    private readonly EvDbStream _evDbStream;
    private readonly IEvDbEventMeta _relatedEventMeta;
    private readonly TimeProvider _timeProvider;
    private readonly JsonSerializerOptions? _options;

    protected OutboxHandlerBase(
        EvDbStream evDbStream,
        IEvDbEventMeta relatedEventMeta)
    {
        _evDbStream = evDbStream;
        _relatedEventMeta = relatedEventMeta;
        _timeProvider = _evDbStream.TimeProvider;
        _options = _evDbStream.Options;
    }

    public void Add<T>(T payload)
        where T : IEvDbPayload
    {
        var json = JsonSerializer.Serialize(payload, _options);
        EvDbOutboxEntity e = new EvDbOutboxEntity(
                                    _relatedEventMeta.EventType, 
                                    payload.PayloadType,
                                    _timeProvider.GetUtcNow(),
                                    _relatedEventMeta.CapturedBy,
                                    _relatedEventMeta.StreamCursor,
                                    json);

        _evDbStream.AddToOutbox(e);
    }
}
