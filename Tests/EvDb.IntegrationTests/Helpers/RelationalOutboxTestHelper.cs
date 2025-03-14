namespace EvDb.Core.Tests;

using Dapper;
using EvDb.Core;
using EvDb.Core.Adapters;
using System.Data.Common;

internal static class RelationalOutboxTestHelper
{
    #region GetOutboxAsync

    public static async IAsyncEnumerable<EvDbMessageRecord> GetOutboxAsync(
        StoreType storeType,
        EvDbStorageContext storageContext,
        EvDbShardName shard)
    {
        #region var outboxQuery = ...

        Func<string, string> toSnakeCase = EvDbStoreNamingPolicy.Default.ConvertName;

        string escape = storeType switch
        {
            StoreType.Postgres => "\"",
            _ => string.Empty
        };

        var outboxQuery = $$"""
                SELECT
                    {{toSnakeCase(nameof(EvDbMessageRecord.Domain))}} as {{nameof(EvDbMessageRecord.Domain)}},
                    {{toSnakeCase(nameof(EvDbMessageRecord.Partition))}} as {{nameof(EvDbMessageRecord.Partition)}},
                    {{toSnakeCase(nameof(EvDbMessageRecord.StreamId))}} as {{nameof(EvDbMessageRecord.StreamId)}},                    
                    {{escape}}{{toSnakeCase(nameof(EvDbMessageRecord.Offset))}}{{escape}} as {{nameof(EvDbMessageRecord.Offset)}},
                    {{toSnakeCase(nameof(EvDbMessageRecord.EventType))}} as {{nameof(EvDbMessageRecord.EventType)}},
                    {{toSnakeCase(nameof(EvDbMessageRecord.Channel))}} as {{nameof(EvDbMessageRecord.Channel)}},
                    {{toSnakeCase(nameof(EvDbMessageRecord.MessageType))}} as {{nameof(EvDbMessageRecord.MessageType)}},
                    {{toSnakeCase(nameof(EvDbMessageRecord.SerializeType))}} as {{nameof(EvDbMessageRecord.SerializeType)}},
                    {{toSnakeCase(nameof(EvDbMessageRecord.CapturedAt))}} as {{nameof(EvDbMessageRecord.CapturedAt)}},
                    {{toSnakeCase(nameof(EvDbMessageRecord.CapturedBy))}} as {{nameof(EvDbMessageRecord.CapturedBy)}},
                    {{toSnakeCase(nameof(EvDbMessageRecord.SpanId))}} as {{nameof(EvDbMessageRecord.SpanId)}},
                    {{toSnakeCase(nameof(EvDbMessageRecord.TraceId))}} as {{nameof(EvDbMessageRecord.TraceId)}},
                    {{toSnakeCase(nameof(EvDbMessageRecord.Payload))}} as {{nameof(EvDbMessageRecord.Payload)}}                  
                FROM 
                    {{storageContext.Id}}{0} 
                ORDER BY 
                    {{escape}}{{toSnakeCase(nameof(EvDbMessageRecord.Offset))}}{{escape}}, 
                    {{toSnakeCase(nameof(EvDbMessageRecord.MessageType))}};
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
