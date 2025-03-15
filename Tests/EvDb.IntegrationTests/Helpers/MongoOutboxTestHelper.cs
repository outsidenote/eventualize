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

        var outboxCollectionFormat = $"{collectionPrefix}{shard}outbox";
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
                EvDbMessageRecord rec = doc.ToMessageRecord();
                yield return rec;
            }
        }
    }

    #endregion //  GetOutboxFromMongoDBAsync
}
