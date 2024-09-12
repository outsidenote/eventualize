using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EvDb.Core.Internals;

public abstract class PublicationProducerBase
{
    private readonly EvDbStream _evDbStream;
    private readonly IEvDbEventMeta _relatedEventMeta;
    private readonly TimeProvider _timeProvider;
    private readonly JsonSerializerOptions? _options;

    public PublicationProducerBase(
        EvDbStream evDbStream,
        IEvDbEventMeta relatedEventMeta)
    {
        _evDbStream = evDbStream;
        _relatedEventMeta = relatedEventMeta;
        _timeProvider = _evDbStream.TimeProvider;
        _options = _evDbStream.Options;
    }

    public void Publish<T>(T payload)
        where T : IEvDbEventPayload
    {
        var json = JsonSerializer.Serialize(payload, _options);
        EvDbEvent e = new EvDbEvent(_relatedEventMeta.EventType,
                                    _timeProvider.GetUtcNow(),
                                    _relatedEventMeta.CapturedBy,
                                    _relatedEventMeta.StreamCursor,
                                    json);

        _evDbStream.Publish(e);
    }

}
