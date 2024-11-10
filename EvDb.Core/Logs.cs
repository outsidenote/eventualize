using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace EvDb.Core;
internal static partial class Logs
{
    [LoggerMessage(LogLevel.Debug, """
                    EvDb find multiple Outbox serialization per context.
                    Channel: {channel}
                    Table Name: {shardName}
                    Payload Type {payloadType}
                    Serializers matched for this context are:
                    {serializers}
                    """)]
    public static partial void LogMultiOutboxSerializers(this ILogger logger,
                                                         string channel,
                                                         string shardName,
                                                         string? payloadType,
                                                         string serializers);
}
