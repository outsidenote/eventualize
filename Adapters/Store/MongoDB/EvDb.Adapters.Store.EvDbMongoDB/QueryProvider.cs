﻿// Ignore Spelling: Sharding
// TBD: [bnaya 2025-04-17] Consider to using Atlas search instead of multiple indexes https://www.mongodb.com/docs/atlas/atlas-search/tutorial/#create-the--index.

using EvDb.Core;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Immutable;

namespace EvDb.Adapters.Store.MongoDB.Internals;

public static class QueryProvider
{
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

    // TODO: [bnaya 2025-04-17] enable to get the Capped from out side, MaxDocuments, MaxSize, TTL (NOT for TS)
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

    #region EventsIndexes

    public static readonly IImmutableList<CreateIndexModel<BsonDocument>> EventsIndexes = [
        Builders<BsonDocument>.IndexKeys
                .Ascending(EvDbFields.Event.Domain)
                .Ascending(EvDbFields.Event.Partition)
                .Ascending(EvDbFields.Event.StreamId)
                .Ascending(EvDbFields.Event.Offset)
            .ToCreateIndexModel("evb_events_idx", true),
        Builders<BsonDocument>.IndexKeys
                .Ascending(EvDbFields.Event.EventType)
                .Ascending(EvDbFields.Event.Domain)
                .Ascending(EvDbFields.Event.Partition)
                .Ascending(EvDbFields.Event.StreamId)
                .Ascending(EvDbFields.Event.Offset)
            .ToCreateIndexModel("evb_events_type_idx", true),
        Builders<BsonDocument>.IndexKeys
                .Ascending(EvDbFields.Event.CapturedAt)
            .ToCreateIndexModel("evb_events_create_at_idx", false),
        ];

    #endregion //  EventsIndexes

    #region OutboxIndexes

    public static readonly IImmutableList<CreateIndexModel<BsonDocument>> OutboxIndexes = CreateOutboxIndexes();

    private static IImmutableList<CreateIndexModel<BsonDocument>> CreateOutboxIndexes()
    {
        return [
            Builders<BsonDocument>.IndexKeys
                .Ascending(EvDbFields.Event.Domain)
                .Ascending(EvDbFields.Event.Partition)
                .Ascending(EvDbFields.Event.StreamId)
                .Ascending(EvDbFields.Event.Offset)
                .ToCreateIndexModel( "evb_outbox_idx", true),
            Builders<BsonDocument>.IndexKeys
                .Ascending(EvDbFields.Event.Domain)
                .Ascending(EvDbFields.Event.Partition)
                .Ascending(EvDbFields.Event.CapturedAt) // sort from here
                .Ascending(EvDbFields.Event.StreamId)
                .Ascending(EvDbFields.Event.Offset)
                .ToCreateIndexModel( "evb_read_pattern_idx"),
            Builders<BsonDocument>.IndexKeys
                .Ascending(EvDbFields.Outbox.Channel)
                .Ascending(EvDbFields.Event.Domain)
                .Ascending(EvDbFields.Event.Partition)
                .Ascending(EvDbFields.Event.CapturedAt) // sort from here
                .Ascending(EvDbFields.Event.StreamId)
                .Ascending(EvDbFields.Event.Offset)
                .ToCreateIndexModel( "evb_read_pattern_Channel_idx"),
            Builders<BsonDocument>.IndexKeys
                .Ascending(EvDbFields.Outbox.MessageType)
                .Ascending(EvDbFields.Event.Domain)
                .Ascending(EvDbFields.Event.Partition)
                .Ascending(EvDbFields.Event.CapturedAt) // sort from here
                .Ascending(EvDbFields.Event.StreamId)
                .Ascending(EvDbFields.Event.Offset)
                .ToCreateIndexModel( "evb_read_pattern_message_type_idx"),
           ];
    }

    #endregion //  OutboxIndexes

    #region SnapshotIndexes

    public static readonly IImmutableList<CreateIndexModel<BsonDocument>> SnapshotIndexes = [
        Builders<BsonDocument>.IndexKeys
                    .Ascending(EvDbFields.Snapshot.Domain)
                    .Ascending(EvDbFields.Snapshot.Partition)
                    .Ascending(EvDbFields.Snapshot.StreamId)
                    .Ascending(EvDbFields.Snapshot.ViewName)
                    .Descending(EvDbFields.Snapshot.Offset)
            .ToCreateIndexModel("evb_snapshots_idx", true)
        ];

    #endregion //  SnapshotIndexes

    #region CreateEnableShardingCommand

    public static BsonDocument CreateEnableShardingCommand(string databaseName) =>
                                new BsonDocument { ["enableSharding"] = databaseName };

    #endregion //  CreateEnableShardingCommand

    #region Sharding

    public static readonly BsonDocument Sharding = CreateSharding();

    private static BsonDocument CreateSharding()
    {
        var sharding = new BsonDocument
        {
            [EvDbFields.Event.Domain] = 1,
            [EvDbFields.Event.Partition] = 1,
            [EvDbFields.Event.EventType] = 1
        };

        return sharding;
    }

    #endregion //  Sharding

    #region SortEvents

    public static SortDefinition<BsonDocument> SortEvents { get; } =
                                    Builders<BsonDocument>.Sort
                                            .Ascending(EvDbFields.Event.Domain)
                                            .Ascending(EvDbFields.Event.Partition)
                                            .Ascending(EvDbFields.Event.StreamId)
                                            .Ascending(EvDbFields.Event.Offset);

    #endregion //  SortEvents

    #region SortSnapshots

    public static SortDefinition<BsonDocument> SortSnapshots { get; } =
                                    Builders<BsonDocument>.Sort
                                            .Ascending(EvDbFields.Snapshot.Domain)
                                            .Ascending(EvDbFields.Snapshot.Partition)
                                            .Ascending(EvDbFields.Snapshot.StreamId)
                                            .Ascending(EvDbFields.Snapshot.ViewName)
                                            .Descending(EvDbFields.Snapshot.Offset);

    #endregion //  SortSnapshots

    #region ProjectionSnapshots

    public static ProjectionDefinition<BsonDocument> ProjectionSnapshots { get; } =
                                    Builders<BsonDocument>.Projection
                                            .Include(EvDbFields.Snapshot.Offset)
                                            .Include(EvDbFields.Snapshot.State);

    #endregion //  ProjectionSnapshots

    #region ToFilter

    public static FilterDefinition<BsonDocument> ToFilter(this EvDbStreamCursor address)
    {
        FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter
                                    .And(
                                        Builders<BsonDocument>.Filter
                                            .Eq(EvDbFields.Event.Domain, address.Domain),
                                        Builders<BsonDocument>.Filter
                                            .Eq(EvDbFields.Event.Partition, address.Partition),
                                        Builders<BsonDocument>.Filter
                                            .Eq(EvDbFields.Event.StreamId, address.StreamId),
                                        Builders<BsonDocument>.Filter
                                            .Gte(EvDbFields.Event.Offset, address.Offset));

        return filter;
    }

    public static FilterDefinition<BsonDocument> ToFilter(this EvDbViewAddress address)
    {
        FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter
                                    .And(
                                        Builders<BsonDocument>.Filter
                                            .Eq(EvDbFields.Snapshot.Domain, address.Domain),
                                        Builders<BsonDocument>.Filter
                                            .Eq(EvDbFields.Snapshot.Partition, address.Partition),
                                        Builders<BsonDocument>.Filter
                                            .Eq(EvDbFields.Snapshot.StreamId, address.StreamId),
                                        Builders<BsonDocument>.Filter
                                            .Eq(EvDbFields.Snapshot.ViewName, address.ViewName));

        return filter;
    }

    #endregion //  ToFilter
}
