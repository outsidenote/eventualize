// Ignore Spelling: Mongo

namespace EvDb.Core.Tests;

using EvDb.Adapters.Store.Internals;
using EvDb.Adapters.Store.MongoDB.Internals;
using EvDb.Core;
using EvDb.Core.Adapters;
using MongoDB.Bson;
using MongoDB.Driver;

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

        string separator = "_";
        if (shard.Value == "outbox")
            shard = string.Empty;
        if (string.IsNullOrEmpty(shard.Value))
            separator = "";
        var outboxCollectionFormat = $"{collectionPrefix}{shard}{separator}outbox";
        var collection = db.GetCollection<BsonDocument>(outboxCollectionFormat);

        var projection = Builders<BsonDocument>.Projection
            .Include(EvDbFields.Outbox.Domain)
            .Include(EvDbFields.Outbox.Domain)
            .Include(EvDbFields.Outbox.Partition)
            .Include(EvDbFields.Outbox.StreamId)
            .Include(EvDbFields.Outbox.Offset)
            .Include(EvDbFields.Outbox.EventType)
            .Include(EvDbFields.Outbox.Channel)
            .Include(EvDbFields.Outbox.MessageType)
            .Include(EvDbFields.Outbox.SerializeType)
            .Include(EvDbFields.Outbox.CapturedAt)
            .Include(EvDbFields.Outbox.CapturedBy)
            .Include(EvDbFields.Outbox.SpanId)
            .Include(EvDbFields.Outbox.TraceId)
            .Include(EvDbFields.Outbox.Payload);

        var sort = Builders<BsonDocument>.Sort
            .Ascending(EvDbFields.Outbox.Offset)
            .Ascending(EvDbFields.Outbox.MessageType);

        IFindFluent<BsonDocument, BsonDocument> query = collection.Find(new BsonDocument())
                             .Sort(sort);
        IAsyncCursor<BsonDocument> cursor = await query.ToCursorAsync();

        while (await cursor.MoveNextAsync())
        {
            foreach (var doc in cursor.Current)
            {
                // Convert from BsonDocument back to EvDbEvent.
                EvDbMessageRecord rec = doc.ToMessageRecord();
                yield return rec;
            }
        }
    }

    #endregion //  GetOutboxFromMongoDBAsync
}
