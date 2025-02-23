using System.Collections.Immutable;

namespace EvDb.Core.Adapters;

public static class EvDbStorageAbstractionsExtensions
{
    public static IEnumerable<IGrouping<EvDbShardName, EvDbMessageRecord>> GroupByShards(
                        this IImmutableList<EvDbMessage> messages,
                        IEnumerable<IEvDbOutboxTransformer> transformers)
    {
        IEnumerable<IGrouping<EvDbShardName, EvDbMessageRecord>> result =
                from transformer in transformers
                from message in messages
                let newPayload = transformer.Transform(message.Channel,
                                                message.MessageType,
                                                message.EventType,
                                                message.Payload)
                let newMessage = message with { Payload = newPayload }
                group (EvDbMessageRecord)newMessage by message.ShardName;
        return result;
    }
}