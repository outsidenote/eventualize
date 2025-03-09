using EvDb.Adapters.Store.Internals;
using EvDb.Core;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections;
using System.Collections.Concurrent;

namespace EvDb.Adapters.Store.EvDbMongoDB.Internals;

public static class QueryProvider
{
    public const string EventsPKName = "evb_events_idx";
    public const string SnapshotsPKName = "evb_snapshots_idx";

    public static readonly MongoCollectionSettings EventsCollectionSetting = new MongoCollectionSettings
    {// Ask
        AssignIdOnInsert = false,
        ReadConcern = ReadConcern.Majority,
        ReadPreference = ReadPreference.Primary,
        WriteConcern = WriteConcern.WMajority,
    }.Freeze();

    public static readonly MongoCollectionSettings SnapshotCollectionSetting = new MongoCollectionSettings
    {// Ask
        AssignIdOnInsert = false,
        //ReadConcern = ReadConcern.Majority, // ASK ReadConcern.Linearizable, 
        ReadPreference = ReadPreference.Nearest,
        WriteConcern = WriteConcern.Acknowledged,
    }.Freeze();

    #region EventsPK

    public static readonly CreateIndexModel<BsonDocument> EventsPK = CreateEventsPK();

    private static CreateIndexModel<BsonDocument> CreateEventsPK()
    {
        // Ask: Go over CreateIndexOptions props?

        IndexKeysDefinition<BsonDocument> indexKeysDefinition = Builders<BsonDocument>.IndexKeys
            .Ascending(EvDbFileds.Event.Domain)
            .Ascending(EvDbFileds.Event.Partition)
            .Ascending(EvDbFileds.Event.StreamId)
            .Ascending(EvDbFileds.Event.Offset);

        var options = new CreateIndexOptions
        {
            Name = EventsPKName
        };
        return new CreateIndexModel<BsonDocument>(indexKeysDefinition, options);
    }

    #endregion //  EventsPK

    private static BsonDocument CreateEnableSahrdingCommand(string databaseName) =>
                                new BsonDocument { { "enableSharding", databaseName } };

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
            .Ascending(EvDbFileds.Snapshot.Domain)
            .Ascending(EvDbFileds.Snapshot.Partition)
            .Ascending(EvDbFileds.Snapshot.StreamId)
            .Ascending(EvDbFileds.Snapshot.ViewName)
            .Descending(EvDbFileds.Snapshot.Offset);

        var options = new CreateIndexOptions
        {
            Name = SnapshotsPKName
        };
        return new CreateIndexModel<BsonDocument>(indexKeysDefinition, options);
    }

    #endregion //  SnapshotPK

    #region SortEvents

    public static SortDefinition<BsonDocument> SortEvents { get; } =
                                    Builders<BsonDocument>.Sort
                                            .Ascending(EvDbFileds.Event.Domain)
                                            .Ascending(EvDbFileds.Event.Partition)
                                            .Ascending(EvDbFileds.Event.StreamId)
                                            .Ascending(EvDbFileds.Event.Offset);

    #endregion //  SortEvents

    #region SortSnapshots

    public static SortDefinition<BsonDocument> SortSnapshots { get; } =
                                    Builders<BsonDocument>.Sort
                                            .Ascending(EvDbFileds.Snapshot.Domain)
                                            .Ascending(EvDbFileds.Snapshot.Partition)
                                            .Ascending(EvDbFileds.Snapshot.StreamId)
                                            .Descending(EvDbFileds.Snapshot.Offset);

    #endregion //  SortSnapshots

    #region ToFilter

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
                                            .Gte(EvDbFileds.Event.Offset, address.Offset));

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
                                            .Eq(EvDbFileds.Snapshot.ViewName, address.ViewName));

        return filter;
    }

    #endregion //  ToFilter
}
