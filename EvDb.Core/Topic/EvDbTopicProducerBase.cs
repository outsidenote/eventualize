// Ignore Spelling: TopicProducer Topic

using System.Text.Json;

namespace EvDb.Core.Internals;

public abstract class EvDbTopicContextBase : IEvDbTopicProducerGeneric
{
    private readonly EvDbStream _evDbStream;
    private readonly IEvDbEventMeta _relatedEventMeta;
    private readonly TimeProvider _timeProvider;
    private readonly JsonSerializerOptions? _options;

    protected EvDbTopicContextBase(
        EvDbStream evDbStream,
        IEvDbEventMeta relatedEventMeta)
    {
        _evDbStream = evDbStream;
        _relatedEventMeta = relatedEventMeta;
        _timeProvider = _evDbStream.TimeProvider;
        _options = _evDbStream.Options;
    }

    public void Add<T>(T payload, string topic)
        where T : IEvDbPayload
    {
        var json = JsonSerializer.Serialize(payload, _options);
        EvDbMessage e = new EvDbMessage(
                                    _relatedEventMeta.EventType,
                                    topic,
                                    payload.PayloadType,
                                    _timeProvider.GetUtcNow(),
                                    _relatedEventMeta.CapturedBy,
                                    _relatedEventMeta.StreamCursor,
                                    json);

        _evDbStream.AddToTopic(e);
    }
}
