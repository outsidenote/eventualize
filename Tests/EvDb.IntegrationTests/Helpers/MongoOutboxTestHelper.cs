// Ignore Spelling: Mongo

using EvDb.Adapters.Store.Internals;
using EvDb.Core.Adapters;
using MongoDB.Bson;
using MongoDB.Driver;
using static EvDb.Core.Adapters.Internals.EvDbStoreNames;

namespace EvDb.Core.Tests;

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
            .Include(Fields.Message.StreamType)
            .Include(Fields.Message.StreamType)
            .Include(Fields.Message.StreamId)
            .Include(Fields.Message.Offset)
            .Include(Fields.Message.EventType)
            .Include(Fields.Message.Channel)
            .Include(Fields.Message.MessageType)
            .Include(Fields.Message.SerializeType)
            .Include(Fields.Message.CapturedAt)
            .Include(Fields.Message.CapturedBy)
            .Include(Fields.Message.TraceParent)
            .Include(Fields.Message.Payload);

        var sort = Builders<BsonDocument>.Sort
            .Ascending(Fields.Message.Offset)
            .Ascending(Fields.Message.MessageType);

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
