// Ignore Spelling: Mongo

namespace EvDb.Core.Tests;

using Dapper;
using EvDb.Adapters.Store.EvDbMongoDB.Internals;
using EvDb.Adapters.Store.Internals;
using EvDb.Core;
using EvDb.Core.Adapters;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Data.Common;
using System.Text.Json;

internal static class MongoOutboxTestHelper
{
    #region GetOutboxFromMongoDBAsync

    public static async IAsyncEnumerable<EvDbMessageRecord> GetOutboxFromMongoDBAsync(
        this EvDbStorageContext storageContext,
        EvDbShardName shard)
    {
        string collectionPrefix = storageContext.CalcCollectionPrefix();
        string connectionString = StoreType.MongoDB.GetConnectionString();

        MongoClientSettings settings = MongoClientSettings.FromConnectionString(connectionString);

        var client = new MongoClient(settings);
        string databaseName = storageContext.DatabaseName;
        var db = client.GetDatabase(databaseName);


        var outboxCollectionFormat = $"""{collectionPrefix}{shard}outbox""";
        var collection = db.GetCollection<BsonDocument>(outboxCollectionFormat);

        var projection = Builders<BsonDocument>.Projection
            .Include(EvDbFileds.Outbox.Domain)
            .Include(EvDbFileds.Outbox.Domain)
            .Include(EvDbFileds.Outbox.Partition)
            .Include(EvDbFileds.Outbox.StreamId)
            .Include(EvDbFileds.Outbox.Offset)
            .Include(EvDbFileds.Outbox.EventType)
            .Include(EvDbFileds.Outbox.Channel)
            .Include(EvDbFileds.Outbox.MessageType)
            .Include(EvDbFileds.Outbox.SerializeType)
            .Include(EvDbFileds.Outbox.CapturedAt)
            .Include(EvDbFileds.Outbox.CapturedBy)
            .Include(EvDbFileds.Outbox.SpanId)
            .Include(EvDbFileds.Outbox.TraceId)
            .Include(EvDbFileds.Outbox.Payload);

        var sort = Builders<BsonDocument>.Sort
            .Ascending(EvDbFileds.Outbox.Offset)
            .Ascending(EvDbFileds.Outbox.MessageType);

        IFindFluent<BsonDocument, BsonDocument> query = collection.Find(new BsonDocument())
                             .Sort(sort);
        IAsyncCursor<BsonDocument> cursor = await query.ToCursorAsync();

        while (await cursor.MoveNextAsync())
        {
            foreach (var doc in cursor.Current)
            {
                // Convert from BsonDocument back to EvDbEvent.
                var rec = doc.ToMessageRecord();
                yield return rec;
            }
        }
    }

    #endregion //  GetOutboxFromMongoDBAsync

    #region GetOutboxAsync

    public static async IAsyncEnumerable<EvDbMessageRecord> GetOutboxFromRelationalDBAsync(
        this StoreType storeType,
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

    #endregion //  GetOutboxFromRelationalDBAsync
}
