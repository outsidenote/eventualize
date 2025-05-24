using Dapper;
using EvDb.Core.Adapters;
using System.Data.Common;

using static EvDb.Core.Adapters.Internals.EvDbStoreNames;

namespace EvDb.Core.Tests;

internal static class RelationalOutboxTestHelper
{
    #region GetOutboxAsync

    public static async IAsyncEnumerable<EvDbMessageRecord> GetOutboxAsync(
        StoreType storeType,
        EvDbStorageContext storageContext,
        EvDbShardName shard)
    {
        #region var outboxQuery = ...

        string escape = storeType switch
        {
            StoreType.Postgres => "\"",
            _ => string.Empty
        };

        var outboxQuery = $$"""
                SELECT
                    {{Fields.Message.StreamType}} as {{Projection.Message.StreamType}},
                    {{Fields.Message.StreamId}} as {{Projection.Message.StreamId}},                    
                    {{escape}}{{Fields.Message.Offset}}{{escape}} as {{Projection.Message.Offset}},
                    {{Fields.Message.EventType}} as {{Projection.Message.EventType}},
                    {{Fields.Message.Channel}} as {{Projection.Message.Channel}},
                    {{Fields.Message.MessageType}} as {{Projection.Message.MessageType}},
                    {{Fields.Message.SerializeType}} as {{Projection.Message.SerializeType}},
                    {{Fields.Message.CapturedAt}} as {{Projection.Message.CapturedAt}},
                    {{Fields.Message.CapturedBy}} as {{Projection.Message.CapturedBy}},
                    {{Fields.Message.TelemetryContext}} as {{Projection.Message.TelemetryContext}},
                    {{Fields.Message.Payload}} as {{Projection.Message.Payload}}                  
                FROM 
                    {{storageContext.Id}}{0} 
                ORDER BY 
                    {{escape}}{{Fields.Message.Offset}}{{escape}}, 
                    {{Fields.Message.MessageType}};
                """;

        #endregion //  var outboxQuery = ...

        using var connection = StoreAdapterHelper.GetConnection(storeType, storageContext);
        await connection.OpenAsync();
        string query = string.Format(outboxQuery, shard);
        DbDataReader reader = await connection.ExecuteReaderAsync(query);
        var parser = reader.GetRowParser<EvDbMessageRecord>();
        while (await reader.ReadAsync())
        {
            EvDbMessageRecord e = parser(reader);
            yield return e;
        }
        await connection.CloseAsync();
    }

    #endregion //  GetOutboxAsync
}
