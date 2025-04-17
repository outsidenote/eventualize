// Ignore Spelling: Sharding

using EvDb.Core;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EvDb.Adapters.Store.MongoDB.Internals;

public static class QueryProvider
{
    public const string EventsPKName = "evb_events_idx";
    public const string SnapshotsPKName = "evb_snapshots_idx";

    #region EventsCollectionSetting

    public static readonly MongoCollectionSettings EventsCollectionSetting = new MongoCollectionSettings
    {
        AssignIdOnInsert = false,
        ReadConcern = ReadConcern.Majority,
        ReadPreference = ReadPreference.PrimaryPreferred,
        WriteConcern = WriteConcern.WMajority,
    }.Freeze();

    #endregion //  EventsCollectionSetting

    #region OutboxCollectionSetting

    public static readonly MongoCollectionSettings OutboxCollectionSetting = new MongoCollectionSettings
    {
        AssignIdOnInsert = false,
        ReadConcern = ReadConcern.Default,
        ReadPreference = ReadPreference.Nearest,
        WriteConcern = WriteConcern.WMajority,
    }.Freeze();

    #endregion //  OutboxCollectionSetting

    #region SnapshotCollectionSetting

    public static readonly MongoCollectionSettings SnapshotCollectionSetting = new MongoCollectionSettings
    {
        AssignIdOnInsert = false,        
        ReadPreference = ReadPreference.Nearest,
        WriteConcern = WriteConcern.Acknowledged,
    }.Freeze();

    #endregion //  SnapshotCollectionSetting

    // TODO: [bnaya 2025-04-17] enable to get the Capped from out side, MaxDocuments, MaxSize (NOT for TS)
    #region DefaultCreateCollectionOptions

    public static CreateCollectionOptions DefaultCreateCollectionOptions { get; } = CreateDefaultCreateCollectionOptions();

    private static CreateCollectionOptions CreateDefaultCreateCollectionOptions()
    {
        var options = new CreateCollectionOptions
        {  
            
            // EncryptedFields
            // IndexOptionDefaults 
        };
        return options;
    }

    #endregion //  DefaultCreateCollectionOptions

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

        // TODO: [bnaya 2025-04-17] get it from outside (offload data story) ExpireAfter (none TS)
        var options = new CreateIndexOptions
        {
            // ExpireAfter
            Name = EventsPKName,
            Unique = true
        };
        return new CreateIndexModel<BsonDocument>(indexKeysDefinition, options);
    }

    #endregion //  EventsPK

    public static BsonDocument CreateEnableShardingCommand(string databaseName) =>
                                new BsonDocument { ["enableSharding"] = databaseName };

    // TODO: [bnaya 2025-04-17] index date-created

    #region OutboxPK

    public static readonly CreateIndexModel<BsonDocument> OutboxPK = CreateOutboxPK();

    private static CreateIndexModel<BsonDocument> CreateOutboxPK()
    {
        IndexKeysDefinition<BsonDocument> indexKeysDefinition = Builders<BsonDocument>.IndexKeys
            .Ascending(EvDbFileds.Event.Domain)
            .Ascending(EvDbFileds.Event.Partition)
            .Ascending(EvDbFileds.Event.StreamId)
            //.Ascending(EvDbFileds.Outbox.Channel)
            //.Ascending(EvDbFileds.Outbox.MessageType)
            .Ascending(EvDbFileds.Event.Offset);

        var options = new CreateIndexOptions
        {
            Name = EventsPKName,
            Unique = true
        };
        return new CreateIndexModel<BsonDocument>(indexKeysDefinition, options);
    }

    // TODO: [bnaya 2025-04-17] Add index for special cases (use Atlas search on Atlas)
    // TODO: [bnaya 2025-04-17] Consider to expose to the user Atlas search https://www.mongodb.com/docs/atlas/atlas-search/tutorial/#create-the--index.
    //private static CreateIndexModel<BsonDocument> CreateOutbox1PK()
    //{
    //    IndexKeysDefinition<BsonDocument> indexKeysDefinition = Builders<BsonDocument>.IndexKeys
    //        .Ascending(EvDbFileds.Event.Domain)
    //        .Ascending(EvDbFileds.Event.Partition)
    //        .Ascending(EvDbFileds.Event.StreamId)
    //        .Ascending(EvDbFileds.Event.Offset);

    //    IndexKeysDefinition<BsonDocument> indexKeysDefinition1 = Builders<BsonDocument>.IndexKeys
    //        .Ascending(EvDbFileds.Outbox.MessageType)
    //        .Ascending(EvDbFileds.Outbox.Channel)
    //        .Ascending(EvDbFileds.Event.Domain)
    //        .Ascending(EvDbFileds.Event.Partition)
    //        .Ascending(EvDbFileds.Event.StreamId)
    //        .Ascending(EvDbFileds.Event.Offset);

    //    IndexKeysDefinition<BsonDocument> indexKeysDefinition2 = Builders<BsonDocument>.IndexKeys
    //        .Ascending(EvDbFileds.Event.Partition)
    //        .Ascending(EvDbFileds.Outbox.MessageType)
    //        .Ascending(EvDbFileds.Event.Domain)
    //        .Ascending(EvDbFileds.Event.StreamId)
    //        .Ascending(EvDbFileds.Event.Offset);

    //    IndexKeysDefinition<BsonDocument> indexKeysDefinition3 = Builders<BsonDocument>.IndexKeys
    //        .Ascending(EvDbFileds.Outbox.Channel)
    //        .Ascending(EvDbFileds.Event.Domain)
    //        .Ascending(EvDbFileds.Event.Partition)
    //        .Ascending(EvDbFileds.Event.StreamId)
    //        .Ascending(EvDbFileds.Event.Offset);

    //    var options = new CreateIndexOptions
    //    {
    //        Name = EventsPKName,
    //        Unique = true
    //    };
    //    return new CreateIndexModel<BsonDocument>(indexKeysDefinition, options);
    //}

    #endregion //  OutboxPK

    #region SnapshotPK

    public static readonly CreateIndexModel<BsonDocument> SnapshotPK = CreateSnapshotPK();

    private static CreateIndexModel<BsonDocument> CreateSnapshotPK()
    {
        IndexKeysDefinition<BsonDocument> indexKeysDefinition = Builders<BsonDocument>.IndexKeys
            .Ascending(EvDbFileds.Snapshot.Domain)
            .Ascending(EvDbFileds.Snapshot.Partition)
            .Ascending(EvDbFileds.Snapshot.StreamId)
            .Ascending(EvDbFileds.Snapshot.ViewName)
            .Descending(EvDbFileds.Snapshot.Offset);

        var options = new CreateIndexOptions
        {
            Name = SnapshotsPKName,
            Unique = true
        };
        return new CreateIndexModel<BsonDocument>(indexKeysDefinition, options);
    }

    #endregion //  SnapshotPK

    #region Sharding

    public static readonly BsonDocument Sharding = CreateSharding();

    private static BsonDocument CreateSharding()
    {
        var sharding = new BsonDocument
        {
            [EvDbFileds.Event.Domain] = 1,
            [EvDbFileds.Event.Partition] = 1,
            [EvDbFileds.Event.EventType] = 1
        };

        return sharding;
    }

    #endregion //  Sharding

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
                                            .Ascending(EvDbFileds.Snapshot.ViewName)
                                            .Descending(EvDbFileds.Snapshot.Offset);

    #endregion //  SortSnapshots

    #region ProjectionSnapshots

    public static ProjectionDefinition<BsonDocument> ProjectionSnapshots { get; } =
                                    Builders<BsonDocument>.Projection
                                            .Include(EvDbFileds.Snapshot.Offset)
                                            .Include(EvDbFileds.Snapshot.State);

    #endregion //  ProjectionSnapshots

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
