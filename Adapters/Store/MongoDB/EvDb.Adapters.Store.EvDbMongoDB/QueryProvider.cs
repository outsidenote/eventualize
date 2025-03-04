using EvDb.Core;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EvDb.Adapters.Store.EvDbMongoDB.Internals;

public static class QueryProvider
{
    public const string EventsPKName = "evb_events_idx";
    public const string SnapshotsPKName = "evb_snapshots_idx";

    #region EventsPK

    public static readonly CreateIndexModel<BsonDocument> EventsPK = CreateEventsPK();

    private static CreateIndexModel<BsonDocument> CreateEventsPK()
    {
        // Ask: Go over CreateIndexOptions props?

        IndexKeysDefinition<BsonDocument> indexKeysDefinition = Builders<BsonDocument>.IndexKeys
            .Hashed(EvDbFileds.Event.Domain)
            .Hashed(EvDbFileds.Event.Partition)
            .Hashed(EvDbFileds.Event.StreamId)
            .Ascending(EvDbFileds.Event.Offset);

        var options = new CreateIndexOptions
        {
            Name = EventsPKName
        };
        return new CreateIndexModel<BsonDocument>(indexKeysDefinition, options);
    }

    #endregion //  EventsPK

    #region // OutboxPK

    //public static readonly IndexKeysDefinition<BsonDocument> OutboxPK = CreateOutboxPK();

    //private static IndexKeysDefinition<BsonDocument> CreateOutboxPK()
    //{
    //    IndexKeysDefinition<BsonDocument> outboxPK = Builders<BsonDocument>.IndexKeys
    //        .Ascending(EvDbFileds.Outbox.CapturedAt)
    //        .Ascending(EvDbFileds.Outbox.Domain)
    //        .Ascending(EvDbFileds.Outbox.Partition)
    //        .Ascending(EvDbFileds.Outbox.StreamId)
    //        .Ascending(EvDbFileds.Outbox.Offset)
    //        .Ascending(EvDbFileds.Outbox.Channel)
    //        .Ascending(EvDbFileds.Outbox.MessageType);

    //    return outboxPK;
    //}

    #endregion //  OutboxPK

    #region SnapshotPK

    public static readonly CreateIndexModel<BsonDocument> SnapshotPK = CreateSnapshotPK();

    private static CreateIndexModel<BsonDocument> CreateSnapshotPK()
    {
        // Ask: how do I make sure that the read use this index?
        IndexKeysDefinition<BsonDocument> indexKeysDefinition = Builders<BsonDocument>.IndexKeys
            .Hashed(EvDbFileds.Snapshot.Domain)
            .Hashed(EvDbFileds.Snapshot.Partition)
            .Hashed(EvDbFileds.Snapshot.StreamId)
            .Hashed(EvDbFileds.Snapshot.ViewName)
            .Descending(EvDbFileds.Snapshot.Offset);

        var options = new CreateIndexOptions
        {
            Name = SnapshotsPKName
        };
        return new CreateIndexModel<BsonDocument>(indexKeysDefinition, options);
    }

    #endregion //  SnapshotPK

    #region ToFilter

    public static FilterDefinition<BsonDocument> ToFilter(this EvDbStreamAddress address)
    {
        FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter
                                    .And(
                                        Builders<BsonDocument>.Filter
                                            .Eq(EvDbFileds.Event.Domain, address.Domain),
                                        Builders<BsonDocument>.Filter
                                            .Eq(EvDbFileds.Event.Partition, address.Partition),
                                        Builders<BsonDocument>.Filter
                                            .Eq(EvDbFileds.Event.StreamId, address.StreamId),
                                        Builders<BsonDocument>.Filter
                                            .Gte("_id", address.ToString()));

        return filter;
    }

    public static FilterDefinition<BsonDocument> ToFilter(this EvDbStreamCursor address)
    {
        FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter
                                    .And(
                                        Builders<BsonDocument>.Filter
                                            .Eq(EvDbFileds.Event.Domain, address.Domain),
                                        Builders<BsonDocument>.Filter
                                            .Eq(EvDbFileds.Event.Partition, address.Partition),
                                        Builders<BsonDocument>.Filter
                                            .Eq(EvDbFileds.Event.StreamId, address.StreamId),
                                        Builders<BsonDocument>.Filter
                                            .Gte("_id", address.ToString()));

        return filter;
    }

    public static FilterDefinition<BsonDocument> ToFilter(this EvDbViewAddress address)
    {
        FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter
                                    .And(
                                        Builders<BsonDocument>.Filter
                                            .Eq(EvDbFileds.Snapshot.Domain, address.Domain),
                                        Builders<BsonDocument>.Filter
                                            .Eq(EvDbFileds.Snapshot.Partition, address.Partition),
                                        Builders<BsonDocument>.Filter
                                            .Eq(EvDbFileds.Snapshot.StreamId, address.StreamId),
                                        Builders<BsonDocument>.Filter
                                            .Eq(EvDbFileds.Snapshot.ViewName, address.ViewName),
                                        Builders<BsonDocument>.Filter
                                            .Gte("_id", address.ToString()));

        return filter;
    }

    #endregion //  ToFilter
}
