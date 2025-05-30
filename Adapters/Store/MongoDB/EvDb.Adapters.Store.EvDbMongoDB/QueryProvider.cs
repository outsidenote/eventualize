// Ignore Spelling: Sharding Bson
// TBD: [bnaya 2025-04-17] Consider to using Atlas search instead of multiple indexes https://www.mongodb.com/docs/atlas/atlas-search/tutorial/#create-the--index.

using EvDb.Core;
using EvDb.Core.Adapters.Internals;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Immutable;
using static EvDb.Core.Adapters.Internals.EvDbStoreNames;
using static EvDb.Core.EvDbStreamAddress;

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

    #region EventsIndexes

    public static readonly IImmutableList<CreateIndexModel<BsonDocument>> EventsIndexes = [
        Builders<BsonDocument>.IndexKeys
                .Ascending(Fields.Event.StreamType)
                .Ascending(Fields.Event.StreamId)
                .Ascending(Fields.Event.Offset)
            .ToCreateIndexModel("evb_events_idx", true),
        //Builders<BsonDocument>.IndexKeys
        //        .Ascending(Fields.Event.EventType)
        //        .Ascending(Fields.Event.StreamType)
        //        .Ascending(Fields.Event.StreamId)
        //        .Ascending(Fields.Event.Offset)
        //    .ToCreateIndexModel("evb_events_type_idx"),
        //Builders<BsonDocument>.IndexKeys
        //        .Ascending(Fields.Event.StoredAt)
        //    .ToCreateIndexModel("evb_events_create_at_idx"),
        ];

    #endregion //  EventsIndexes

    #region OutboxIndexes

    public static readonly IImmutableList<CreateIndexModel<BsonDocument>> OutboxIndexes = CreateOutboxIndexes();

    private static IImmutableList<CreateIndexModel<BsonDocument>> CreateOutboxIndexes()
    {
        IndexKeysDefinition<BsonDocument> baseIndex =
            Builders<BsonDocument>.IndexKeys
                .Ascending(Fields.Message.StreamType)
                .Ascending(Fields.Message.StreamId);
        return [
            baseIndex
                .Ascending(Fields.Message.Offset)
                .ToCreateIndexModel( "evb_outbox_idx", true),
            baseIndex
                .Ascending(Fields.Message.StoredAt)
                .Ascending(Fields.Message.Offset)
                .ToCreateIndexModel( "evb_read_stored_at_idx"),
            //baseIndex
            //    .Ascending(Fields.Message.StoredAt)
            //    .Ascending(Fields.Message.Channel)
            //    .Ascending(Fields.Message.Offset)
            //    .ToCreateIndexModel( "evb_read_channel_capture_at_idx"),
            //baseIndex
            //    .Ascending(Fields.Message.MessageType)
            //    .Ascending(Fields.Message.StoredAt)
            //    .Ascending(Fields.Message.Offset)
            //    .ToCreateIndexModel( "evb_read_message_type_capture_at_idx"),
           ];
    }

    #endregion //  OutboxIndexes

    #region SnapshotIndexes

    public static readonly IImmutableList<CreateIndexModel<BsonDocument>> SnapshotIndexes = [
        Builders<BsonDocument>.IndexKeys
                    .Ascending(Fields.Snapshot.StreamType)
                    .Ascending(Fields.Snapshot.StreamId)
                    .Ascending(Fields.Snapshot.ViewName)
                    .Descending(Fields.Snapshot.Offset)
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
            [Fields.Event.StreamType] = 1,
            [Fields.Event.EventType] = 1
        };

        return sharding;
    }

    #endregion //  Sharding

    #region SortEvents

    public static SortDefinition<BsonDocument> SortEvents { get; } =
                                    Builders<BsonDocument>.Sort
                                            .Ascending(Fields.Event.StreamType)
                                            .Ascending(Fields.Event.StreamId)
                                            .Ascending(Fields.Event.Offset);

    #endregion //  SortEvents

    #region SortEventsDesc

    public static SortDefinition<BsonDocument> SortEventsDesc { get; } =
                                    Builders<BsonDocument>.Sort
                                            .Ascending(Fields.Event.StreamType)
                                            .Ascending(Fields.Event.StreamId)
                                            .Descending(Fields.Event.Offset);

    #endregion //  SortEventsDesc

    #region SortMessages

    public static SortDefinition<BsonDocument> SortMessages { get; } =
                                    Builders<BsonDocument>.Sort
                                            .Ascending(Fields.Message.StoredAt)
                                            .Ascending(Fields.Message.Channel)
                                            .Ascending(Fields.Message.MessageType)
                                            .Ascending(Fields.Message.Offset);

    #endregion //  SortMessages

    #region SortSnapshots

    public static SortDefinition<BsonDocument> SortSnapshots { get; } =
                                    Builders<BsonDocument>.Sort
                                            .Ascending(Fields.Snapshot.StreamType)
                                            .Ascending(Fields.Snapshot.StreamId)
                                            .Ascending(Fields.Snapshot.ViewName)
                                            .Descending(Fields.Snapshot.Offset);

    #endregion //  SortSnapshots

    #region ProjectionOffset

    public static ProjectionDefinition<BsonDocument> ProjectionOffset { get; } =
                                    Builders<BsonDocument>.Projection
                                            .Include(Fields.Event.Offset)
                                            .Exclude("_id");

    #endregion //  ProjectionOffset

    #region ProjectionSnapshots

    public static ProjectionDefinition<BsonDocument> ProjectionSnapshots { get; } =
                                    Builders<BsonDocument>.Projection
                                            .Include(Fields.Snapshot.Offset)
                                            .Include(Fields.Snapshot.State);

    #endregion //  ProjectionSnapshots

    #region ToFilter

    public static FilterDefinition<BsonDocument> ToBsonFilter(this EvDbStreamAddress address)
    {
        FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter
                                    .And(
                                        Builders<BsonDocument>.Filter
                                            .Eq(Fields.Event.StreamType, address.StreamType.Value),
                                        Builders<BsonDocument>.Filter
                                            .Eq(Fields.Event.StreamId, address.StreamId));

        return filter;
    }

    public static FilterDefinition<BsonDocument> ToBsonFilter(this EvDbStreamCursor address)
    {
        FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter
                                    .And(
                                        Builders<BsonDocument>.Filter
                                            .Eq(Fields.Event.StreamType, address.StreamType),
                                        Builders<BsonDocument>.Filter
                                            .Eq(Fields.Event.StreamId, address.StreamId),
                                        Builders<BsonDocument>.Filter
                                            .Gte(Fields.Event.Offset, address.Offset));

        return filter;
    }

    public static FilterDefinition<BsonDocument> ToBsonFilter(this EvDbGetEventsParameters parameters)
    {
        FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter
                                    .And(
                                        Builders<BsonDocument>.Filter
                                            .Eq(Fields.Event.StreamType, parameters.StreamType),
                                        Builders<BsonDocument>.Filter
                                            .Eq(Fields.Event.StreamId, parameters.StreamId),
                                        Builders<BsonDocument>.Filter
                                            .Gte(Fields.Event.Offset, parameters.SinceOffset));

        return filter;
    }

    public static FilterDefinition<BsonDocument> ToBsonFilter(this EvDbViewAddress address)
    {
        FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter
                                    .And(
                                        Builders<BsonDocument>.Filter
                                            .Eq(Fields.Snapshot.StreamType, address.StreamType),
                                        Builders<BsonDocument>.Filter
                                            .Eq(Fields.Snapshot.StreamId, address.StreamId),
                                        Builders<BsonDocument>.Filter
                                            .Eq(Fields.Snapshot.ViewName, address.ViewName));

        return filter;
    }

    public static FilterDefinition<BsonDocument> ToBsonFilter(this EvDbGetMessagesParameters parameters)
    {
        var filters = new List<FilterDefinition<BsonDocument>>
        {
            Builders<BsonDocument>.Filter.Gte(Fields.Message.StoredAt, parameters.SinceDate)
        };

        // Add Channel filter if Channels array is provided and not empty
        if (parameters.Channels is { Length: > 0 })
        {
            filters.Add(Builders<BsonDocument>.Filter
                                              .In(Fields.Message.Channel, parameters.Channels));
        }

        // Add MessageType filter if MessageTypes array is provided and not empty
        if (parameters.MessageTypes is { Length: > 0 })
        {
            filters.Add(Builders<BsonDocument>.Filter
                                              .In(Fields.Message.MessageType, parameters.MessageTypes));
        }

        return Builders<BsonDocument>.Filter.And(filters);
    }

    #endregion //  ToBsonFilter

    #region ToBsonPipeline

    public static PipelineDefinition<ChangeStreamDocument<BsonDocument>, ChangeStreamDocument<BsonDocument>> ToBsonPipeline(
        this EvDbGetMessagesParameters parameters)
    {
        var filters = new List<FilterDefinition<ChangeStreamDocument<BsonDocument>>>
        {
            Builders<ChangeStreamDocument<BsonDocument>>.Filter
                .Eq(cs => cs.OperationType, ChangeStreamOperationType.Insert)
        };

        if (parameters.Channels is { Length: > 0 })
        {
            filters.Add(Builders<ChangeStreamDocument<BsonDocument>>.Filter
                .In("fullDocument.channel", parameters.Channels));
        }

        if (parameters.MessageTypes is { Length: > 0 })
        {
            filters.Add(Builders<ChangeStreamDocument<BsonDocument>>.Filter
                .In("fullDocument.messageType", parameters.MessageTypes));
        }

        var matchStage = Builders<ChangeStreamDocument<BsonDocument>>.Filter.And(filters);

        return PipelineDefinition<ChangeStreamDocument<BsonDocument>, ChangeStreamDocument<BsonDocument>>
            .Create(new[] { PipelineStageDefinitionBuilder.Match(matchStage) });
    }

    #endregion //  ToBsonPipeline
}
