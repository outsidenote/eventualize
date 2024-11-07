// Ignore Spelling: TopicProducer Topic

using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Text.Json;

namespace EvDb.Core.Internals;

public abstract class EvDbOutboxContextBase : IEvDbOutboxProducerGeneric
{
    private readonly ILogger _logger;
    private readonly EvDbOutboxSerializationMode _mode;
    private readonly EvDbStream _evDbStream;
    private readonly IEvDbEventMeta _relatedEventMeta;
    private readonly TimeProvider _timeProvider;
    private readonly JsonSerializerOptions? _options;

    protected EvDbOutboxContextBase(
        ILogger logger,
        EvDbOutboxSerializationMode mode,
        EvDbStream evDbStream,
        IEvDbEventMeta relatedEventMeta)
    {
        _logger = logger;
        _mode = mode;
        _evDbStream = evDbStream;
        _relatedEventMeta = relatedEventMeta;
        _timeProvider = _evDbStream.TimeProvider;
        _options = _evDbStream.Options;
    }

    protected abstract IImmutableList<IEvDbOutboxSerializer> OutboxSerializers { get; }

    public void Add<T>(T payload, string channel, EvDbTableName tableName)
        where T : IEvDbPayload
    {
        if(payload == null)
            throw new ArgumentNullException(nameof(payload));

        IEvDbOutboxSerializer? serializer = null;
        IEvDbOutboxSerializer[] serializers = OutboxSerializers.Where(m =>
                                                    m.ShouldSerialize(channel,
                                                                tableName,
                                                                payload))
                                                    .ToArray();
        #region Validation

        if (serializers.Length > 1)
        {
            if (_mode == EvDbOutboxSerializationMode.Strict)
            {
                throw new InvalidOperationException($"""
                    EvDb Outbox serialization in strict mode expect 
                    a single serializer per context.
                    Channel: {channel}
                    Table Name: {tableName}
                    Payload Type {payload?.GetType().Name}
                    Serializers matched for this context are:
                    {string.Join(", ", serializers.Select(m => m.Name))}
                    """
                    );
            }
        }
        else
        {
            _logger.LogMultiOutboxSerializers(channel, tableName, payload?.GetType().Name,
                string.Join(", ", serializers.Select(m => m.Name)));
        }

        #endregion //  Validation

        if (serializers.Length == 1)
            serializer = serializers[0];

        #region byte[] buffer =  serializer?.Serialize(...) ?? JsonSerializer.SerializeToUtf8Bytes(...)

        byte[] buffer = Array.Empty<byte>();
        if (payload != null)
        {
            if (serializer == null)
                buffer = JsonSerializer.SerializeToUtf8Bytes(payload, _options);

            else
                buffer = serializer.Serialize(channel, tableName, payload);
        }

        #endregion //  byte[] buffer =  serializer?.Serialize(...) ?? JsonSerializer.SerializeToUtf8Bytes(...)

        EvDbMessage e = new EvDbMessage(
                                    _relatedEventMeta.EventType,
                                    channel,
                                    tableName,
                                    payload.PayloadType,
                                    _timeProvider.GetUtcNow(),
                                    _relatedEventMeta.CapturedBy,
                                    _relatedEventMeta.StreamCursor,
                                    buffer);

        _evDbStream.AddToTopic(e);
    }
}
