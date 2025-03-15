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
    {// Ask
        AssignIdOnInsert = false,
        ReadConcern = ReadConcern.Majority,
        ReadPreference = ReadPreference.Primary,
        WriteConcern = WriteConcern.WMajority,
    }.Freeze();

    #endregion //  EventsCollectionSetting

    #region OutboxCollectionSetting

    public static readonly MongoCollectionSettings OutboxCollectionSetting = new MongoCollectionSettings
    {// Ask
        AssignIdOnInsert = false,
        ReadConcern = ReadConcern.Default,
        ReadPreference = ReadPreference.Nearest,
        WriteConcern = WriteConcern.WMajority,
    }.Freeze();

    #endregion //  OutboxCollectionSetting

    #region SnapshotCollectionSetting

    public static readonly MongoCollectionSettings SnapshotCollectionSetting = new MongoCollectionSettings
    {// Ask
        AssignIdOnInsert = false,
        //ReadConcern = ReadConcern.Majority, // ASK ReadConcern.Linearizable, 
        ReadPreference = ReadPreference.Nearest,
        WriteConcern = WriteConcern.Acknowledged,
    }.Freeze();

    #endregion //  SnapshotCollectionSetting

    #region DefaultCreateCollectionOptions

    public static CreateCollectionOptions DefaultCreateCollectionOptions { get; } = CreateDefaultCreateCollectionOptions();

    private static CreateCollectionOptions CreateDefaultCreateCollectionOptions()
    {
        // Ask
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

        var options = new CreateIndexOptions
        {
            Name = EventsPKName,
            Unique = true
        };
        return new CreateIndexModel<BsonDocument>(indexKeysDefinition, options);
    }

    #endregion //  EventsPK

    public static BsonDocument CreateEnableShardingCommand(string databaseName) =>
                                new BsonDocument { ["enableSharding"] = databaseName };

    #region // OutboxPK

    public static readonly CreateIndexModel<BsonDocument> OutboxPK = CreateOutboxPK();

    private static CreateIndexModel<BsonDocument> CreateOutboxPK()
    {
        IndexKeysDefinition<BsonDocument> indexKeysDefinition = Builders<BsonDocument>.IndexKeys
            .Ascending(EvDbFileds.Event.Domain)
            .Ascending(EvDbFileds.Event.Partition)
            .Ascending(EvDbFileds.Event.StreamId)
            .Ascending(EvDbFileds.Event.Offset)
            .Ascending(EvDbFileds.Outbox.Channel)
            .Ascending(EvDbFileds.Outbox.MessageType);

        var options = new CreateIndexOptions
        {
            Name = EventsPKName,
            Unique = true
        };
        return new CreateIndexModel<BsonDocument>(indexKeysDefinition, options);
    }

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

    #region SnapshotPK

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
