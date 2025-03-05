// Ignore Spelling: OutboxProducer Channel

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

    void IEvDbOutboxProducerGeneric.Add<T>(T payload, EvDbChannelName channel, EvDbShardName shardName)
    {
#pragma warning disable S2955 // Generic parameters not constrained to reference types should not be compared to "null"
        if (payload == null)
            throw new ArgumentNullException(nameof(payload));
#pragma warning restore S2955 // Generic parameters not constrained to reference types should not be compared to "null"

        IEvDbOutboxSerializer? serializer = null;
        IEvDbOutboxSerializer[] serializers = OutboxSerializers.Where(m =>
                                                    m.ShouldSerialize(channel,
                                                                shardName,
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
                    SerializerType: {shardName}
                    Payload Type {payload?.PayloadType}
                    Serializers matched for this context are:
                    {string.Join(", ", serializers.Select(m => m.SerializerType))}
                    """
                    );
            }

            _logger.LogMultiOutboxSerializers(channel, shardName, payload?.PayloadType,
                string.Join(", ", serializers.Select(m => m.SerializerType)));
        }

        #endregion //  Validation

        if (serializers.Length > 0)
            serializer = serializers[0];
        string serializerType = serializer?.SerializerType ?? IEvDbOutboxSerializer.DefaultFormat;

        #region byte[] buffer =  serializer?.Serialize(...) ?? JsonSerializer.SerializeToUtf8Bytes(...)

        byte[] buffer = Array.Empty<byte>();
        if (serializer == null)
            buffer = JsonSerializer.SerializeToUtf8Bytes(payload!, _options);

        else
            buffer = serializer.Serialize(channel, shardName, payload!);

        #endregion //  byte[] buffer =  serializer?.Serialize(...) ?? JsonSerializer.SerializeToUtf8Bytes(...)

        EvDbMessage e = new EvDbMessage(
                                    _relatedEventMeta.EventType,
                                    channel,
                                    shardName,
                                    payload!.PayloadType,
                                    serializerType,
                                    _timeProvider.GetUtcNow(),
                                    _relatedEventMeta.CapturedBy,
                                    _relatedEventMeta.StreamCursor,
                                    buffer);

        _evDbStream.AddToOutbox(e);
    }
}
