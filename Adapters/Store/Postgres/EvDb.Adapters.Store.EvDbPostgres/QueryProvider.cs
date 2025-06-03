using EvDb.Core;
using EvDb.Core.Adapters;
using static EvDb.Core.Adapters.Internals.EvDbStoreNames;

namespace EvDb.Adapters.Store.Postgres;

internal static class QueryProvider
{
    /// <summary>
    /// Creates the stream queries.
    /// </summary>
    /// <param name="storageContext">The storage context.</param>
    /// <returns></returns>
    public static EvDbStreamAdapterQueryTemplates CreateStreamQueries(EvDbStorageContext storageContext)
    {
        string schema = storageContext.Schema.HasValue
            ? $"{storageContext.Schema}."
            : string.Empty;
        string tblInitial = $"{schema}{storageContext.ShortId}";

        return new EvDbStreamAdapterQueryTemplates
        {
            GetLastOffset = $"""
                SELECT
                    "{Fields.Event.Offset}" as {Projection.Event.Offset}                  
                FROM {tblInitial}events
                WHERE {Fields.Event.StreamType} = {Parameters.Event.StreamType}
                    AND {Fields.Event.StreamId} = {Parameters.Event.StreamId}
                ORDER BY "{Fields.Event.Offset}" DESC;
                """,
            GetEvents = $"""
                SELECT
                    {Fields.Event.StreamType} as {Projection.Event.StreamType},
                    {Fields.Event.StreamId} as {Projection.Event.StreamId},
                    "{Fields.Event.Offset}" as {Projection.Event.Offset},
                    {Fields.Event.EventType} as {Projection.Event.EventType},
                    {Fields.Event.CapturedAt} as {Projection.Event.CapturedAt},
                    {Fields.Event.StoredAt} as {Projection.Event.StoredAt},
                    {Fields.Event.CapturedBy} as {Projection.Event.CapturedBy},
                    {Fields.Event.Payload} as {Projection.Event.Payload}                  
                FROM {tblInitial}events
                WHERE {Fields.Event.StreamType} = {Parameters.Event.StreamType}
                    AND {Fields.Event.StreamId} = {Parameters.Event.StreamId}
                    AND "{Fields.Event.Offset}" >= {Parameters.Event.SinceOffset}
                ORDER BY "{Fields.Event.Offset}" ASC
                LIMIT {Parameters.Event.BatchSize};
                """,
            GetMessages = $$"""
                SELECT
                    {{Fields.Message.StreamType}} as {{Projection.Message.StreamType}},
                    {{Fields.Message.StreamId}} as {{Projection.Message.StreamId}},
                    "{{Fields.Message.Offset}}" as {{Projection.Message.Offset}},
                    {{Fields.Message.EventType}} as {{Projection.Message.EventType}},
                    {{Fields.Message.MessageType}} as {{Projection.Message.MessageType}},
                    {{Fields.Message.CapturedAt}} as {{Projection.Message.CapturedAt}},
                    {{Fields.Message.StoredAt}} as {{Projection.Message.StoredAt}},
                    {{Fields.Message.CapturedBy}} as {{Projection.Message.CapturedBy}},
                    {{Fields.Message.Channel}} as {{Projection.Message.Channel}},
                    {{Fields.Message.SerializeType}} as {{Projection.Message.SerializeType}},
                    {{Fields.Message.TelemetryContext}} as {{Projection.Message.TelemetryContext}},
                    {{Fields.Message.Payload}} as {{Projection.Message.Payload}}                  
                FROM {{tblInitial}}{0}
                WHERE 
                    {{Fields.Message.StoredAt}} >= {{Parameters.Message.SinceDate}} 
                    AND ({{Fields.Message.Channel}} = ANY({{Parameters.Message.Channels}}) OR {{Parameters.Message.Channels}} IS NULL OR array_length({{Parameters.Message.Channels}}, 1) = 0)
                    AND ({{Fields.Message.MessageType}} = ANY({{Parameters.Message.MessageTypes}}) OR {{Parameters.Message.MessageTypes}} IS NULL OR array_length({{Parameters.Message.MessageTypes}}, 1) = 0)
                ORDER BY {{Fields.Message.StoredAt}} ASC, {{Fields.Message.Channel}} ASC, {{Fields.Message.MessageType}} ASC, "{{Fields.Event.Offset}}" ASC
                LIMIT {{Parameters.Message.BatchSize}};
                """,
            SaveEvents = $$"""
             INSERT INTO {{tblInitial}}events 
                    ({{Fields.Event.Id}}, 
                    {{Fields.Event.StreamType}}, 
                    {{Fields.Event.StreamId}}, 
                    "{{Fields.Event.Offset}}", 
                    {{Fields.Event.EventType}}, 
                    {{Fields.Event.CapturedAt}}, 
                    {{Fields.Event.StoredAt}}, 
                    {{Fields.Event.CapturedBy}}, 
                    {{Fields.Event.TelemetryContext}}, 
                    {{Fields.Event.Payload}})
                SELECT 
                    UNNEST({{Parameters.Event.Id}}), 
                    UNNEST({{Parameters.Event.StreamType}}), 
                    UNNEST({{Parameters.Event.StreamId}}), 
                    UNNEST({{Parameters.Event.Offset}}), 
                    UNNEST({{Parameters.Event.EventType}}), 
                    UNNEST({{Parameters.Event.CapturedAt}}), 
                    NOW() AT TIME ZONE 'UTC', 
                    UNNEST({{Parameters.Event.CapturedBy}}), 
                    UNNEST({{Parameters.Event.TelemetryContext}}), 
                    UNNEST({{Parameters.Event.Payload}})
            """,
            SaveToOutbox = $$"""
             INSERT INTO {{tblInitial}}{0}
                    ({{Fields.Message.Id}}, 
                    {{Fields.Message.StreamType}}, 
                    {{Fields.Message.StreamId}}, 
                    "{{Fields.Message.Offset}}", 
                    {{Fields.Message.Channel}}, 
                    {{Fields.Message.MessageType}}, 
                    {{Fields.Message.SerializeType}}, 
                    {{Fields.Message.EventType}}, 
                    {{Fields.Message.CapturedBy}}, 
                    {{Fields.Message.CapturedAt}}, 
                    {{Fields.Message.StoredAt}}, 
                    {{Fields.Message.TelemetryContext}}, 
                    {{Fields.Message.Payload}})
                SELECT 
                    UNNEST({{Parameters.Message.Id}}), 
                    UNNEST({{Parameters.Message.StreamType}}), 
                    UNNEST({{Parameters.Message.StreamId}}), 
                    UNNEST({{Parameters.Message.Offset}}), 
                    UNNEST({{Parameters.Message.Channel}}), 
                    UNNEST({{Parameters.Message.MessageType}}), 
                    UNNEST({{Parameters.Message.SerializeType}}), 
                    UNNEST({{Parameters.Message.EventType}}), 
                    UNNEST({{Parameters.Message.CapturedBy}}), 
                    UNNEST({{Parameters.Message.CapturedAt}}), 
                    NOW() AT TIME ZONE 'UTC', 
                    UNNEST({{Parameters.Message.TelemetryContext}}), 
                    UNNEST({{Parameters.Message.Payload}})
            """
        };
    }

    public static EvDbSnapshotAdapterQueryTemplates CreateSnapshotQueries(EvDbStorageContext storageContext)
    {
        Func<string, string> toSnakeCase = EvDbStoreNamingPolicy.Default.ConvertName;
        string tabInitial = $"{storageContext.Schema}.{storageContext.ShortId}";

        return new EvDbSnapshotAdapterQueryTemplates
        {
            GetSnapshot = $"""
                SELECT {Fields.Snapshot.State} as {Projection.Snapshot.State}, 
                        "{Fields.Snapshot.Offset}" as {Projection.Snapshot.Offset}
                FROM {tabInitial}snapshot
                WHERE {Fields.Snapshot.StreamType} = {Parameters.Snapshot.StreamType}
                    AND {Fields.Snapshot.StreamId} = {Parameters.Snapshot.StreamId}
                    AND {Fields.Snapshot.ViewName} = {Parameters.Snapshot.ViewName}
                ORDER BY "{Fields.Snapshot.Offset}" DESC
                LIMIT 1;
                """,
            SaveSnapshot = $"""
            INSERT INTO {tabInitial}snapshot (
                        {Fields.Snapshot.Id},
                        {Fields.Snapshot.StreamType},
                        {Fields.Snapshot.StreamId},
                        {Fields.Snapshot.ViewName},
                        "{Fields.Snapshot.Offset}",
                        {Fields.Snapshot.State})
            VALUES (
                        {Parameters.Snapshot.Id},
                        {Parameters.Snapshot.StreamType},
                        {Parameters.Snapshot.StreamId},
                        {Parameters.Snapshot.ViewName},
                        {Parameters.Snapshot.Offset},
                        {Parameters.Snapshot.State})
            """
        };
    }
}
