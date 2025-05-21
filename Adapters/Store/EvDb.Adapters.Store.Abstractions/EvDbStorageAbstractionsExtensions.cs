using System.Collections.Immutable;

namespace EvDb.Core.Adapters;

public static class EvDbStorageAbstractionsExtensions
{
    /// <summary>
    /// Groups the messages by their shard name.
    /// </summary>
    /// <param name="messages"></param>
    /// <param name="transformers"></param>
    /// <returns></returns>
    public static IEnumerable<IGrouping<EvDbShardName, EvDbMessageRecord>> GroupByShards(
                        this IImmutableList<EvDbMessage> messages,
                        IImmutableList<IEvDbOutboxTransformer> transformers)
    {
        if (transformers.Count != 0)
        {
            var transformed = from transformer in transformers
                              from message in messages
                              let newPayload = transformer.Transform(message.Channel,
                                                  message.MessageType,
                                                  message.EventType,
                                                  message.Payload)
                              let newMessage = message with { Payload = newPayload }
                              select newMessage;
            messages = transformed.ToImmutableList();
        }

        IEnumerable<IGrouping<EvDbShardName, EvDbMessageRecord>> result =
                from message in messages
                group (EvDbMessageRecord)message by message.ShardName;
        return result;
    }
}