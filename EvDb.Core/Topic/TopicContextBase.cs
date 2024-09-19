// Ignore Spelling: TopicProducer Topic

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EvDb.Core.Internals;

public abstract class TopicContextBase
{
    private readonly EvDbStream _evDbStream;
    private readonly IEvDbEventMeta _relatedEventMeta;
    private readonly TimeProvider _timeProvider;
    private readonly JsonSerializerOptions? _options;

    protected TopicContextBase(
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
        EvDbMessage e = new EvDbMessage(
                                    _relatedEventMeta.EventType, 
                                    "DEFAULT", // TODO: Bnaya 2024-09-19 get the topic name
                                    payload.PayloadType,
                                    _timeProvider.GetUtcNow(),
                                    _relatedEventMeta.CapturedBy,
                                    _relatedEventMeta.StreamCursor,
                                    json);

        _evDbStream.AddToTopic(e);
    }
}
