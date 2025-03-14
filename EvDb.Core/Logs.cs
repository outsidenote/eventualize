using Microsoft.Extensions.Logging;

namespace EvDb.Core;
internal static partial class Logs
{
    [LoggerMessage(LogLevel.Debug, """
                    EvDb find multiple matching Outbox serialization for the message (IEvDbOutboxSerializer.ShouldSerialize ).
                    All serializer but the first one will be ignored!
                    Channel: {channel}
                    Table Name: {shardName}
                    Payload Type {payloadType}
                    Serializers matched for this context are:
                    {serializers}
                    """)]
    public static partial void LogMultiOutboxSerializers(this ILogger logger,
                                                         string channel,
                                                         EvDbShardName shardName,
                                                         string? payloadType,
                                                         string serializers);
}
