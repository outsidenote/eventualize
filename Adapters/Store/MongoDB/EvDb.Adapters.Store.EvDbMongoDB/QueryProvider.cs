// Ignore Spelling: Sharding Bson
// TBD: [bnaya 2025-04-17] Consider to using Atlas search instead of multiple indexes https://www.mongodb.com/docs/atlas/atlas-search/tutorial/#create-the--index.

using EvDb.Core;
using EvDb.Core.Adapters.Internals;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
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
        return [
            Builders<BsonDocument>.IndexKeys
                .Ascending(Fields.Message.StreamType)
                .Ascending(Fields.Message.StreamId)
                .Ascending(Fields.Message.Offset)
                .Ascending(Fields.Message.Channel)
                .Ascending(Fields.Message.MessageType)
                .ToCreateIndexModel( "evb_outbox_unique_idx", true),
            Builders<BsonDocument>.IndexKeys
                .Ascending("_id")
                .Ascending(Fields.Message.Channel)
                .Ascending(Fields.Message.MessageType)
                .Ascending(Fields.Message.Offset)
                .ToCreateIndexModel( "evb_read_stored_at_idx"),
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
                                            .Ascending("_id")
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

    public static BsonDocument ToBsonFilter(this EvDbGetMessagesParameters parameters,
                                          ObjectId? continueAfter = null)
    {
        var matchFilters = new List<BsonDocument>();

        // ID-based filtering
        var idFilter = new BsonDocument();
        if (continueAfter.HasValue)
        {
            idFilter["$gt"] = continueAfter.Value;
        }
        else
        {
            idFilter["$gte"] = ObjectId.GenerateNewId(parameters.SinceDate.UtcDateTime);
        }
        matchFilters.Add(new BsonDocument("_id", idFilter));

        // Trim the last millisecond to avoid late arrival issues
        matchFilters.Add(new BsonDocument("$expr",
            new BsonDocument("$lt", new BsonArray
            {
                new BsonDocument("$toDate", "$_id"),
                new BsonDocument("$dateSubtract", new BsonDocument
                {
                    { "startDate", "$$NOW" },
                    { "unit", "millisecond" },
                    { "amount", 1 }
                })
            })
        ));

        // Channel filtering
        if (parameters.Channels is { Length: > 0 })
        {
            matchFilters.Add(new BsonDocument(Fields.Message.Channel,
                new BsonDocument("$in", new BsonArray(parameters.Channels))));
        }

        // Message type filtering  
        if (parameters.MessageTypes is { Length: > 0 })
        {
            matchFilters.Add(new BsonDocument(Fields.Message.MessageType,
                new BsonDocument("$in", new BsonArray(parameters.MessageTypes))));
        }

        // Combine filters with $and
        return matchFilters.Count == 1
            ? matchFilters[0]
            : new BsonDocument("$and", new BsonArray(matchFilters));
    }

    #endregion //  ToBsonFilter
}
