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
                WHERE {Fields.Event.Domain} = {Parameters.Event.Domain}
                    AND {Fields.Event.Partition} = {Parameters.Event.Partition}
                    AND {Fields.Event.StreamId} = {Parameters.Event.StreamId}
                ORDER BY "{Fields.Event.Offset}" DESC;
                """,
            GetEvents = $"""
                SELECT
                    {Fields.Event.Domain} as {Projection.Event.Domain},
                    {Fields.Event.Partition} as {Projection.Event.Partition},
                    {Fields.Event.StreamId} as {Projection.Event.StreamId},
                    "{Fields.Event.Offset}" as {Projection.Event.Offset},
                    {Fields.Event.EventType} as {Projection.Event.EventType},
                    {Fields.Event.CapturedAt} as {Projection.Event.CapturedAt},
                    {Fields.Event.CapturedBy} as {Projection.Event.CapturedBy},
                    {Fields.Event.Payload} as {Projection.Event.Payload}                  
                FROM {tblInitial}events
                WHERE {Fields.Event.Domain} = {Parameters.Event.Domain}
                    AND {Fields.Event.Partition} = {Parameters.Event.Partition}
                    AND {Fields.Event.StreamId} = {Parameters.Event.StreamId}
                    AND "{Fields.Event.Offset}" >= {Parameters.Event.Offset};
                """,
            SaveEvents = $$"""
             INSERT INTO {{tblInitial}}events 
                    ({{Fields.Event.Id}}, 
                    {{Fields.Event.Domain}}, 
                    {{Fields.Event.Partition}}, 
                    {{Fields.Event.StreamId}}, 
                    "{{Fields.Event.Offset}}", 
                    {{Fields.Event.EventType}}, 
                    {{Fields.Event.CapturedAt}}, 
                    {{Fields.Event.CapturedBy}}, 
                    {{Fields.Event.TelemetryContext}}, 
                    {{Fields.Event.Payload}})
                SELECT 
                    UNNEST({{Parameters.Event.Id}}), 
                    UNNEST({{Parameters.Event.Domain}}), 
                    UNNEST({{Parameters.Event.Partition}}), 
                    UNNEST({{Parameters.Event.StreamId}}), 
                    UNNEST({{Parameters.Event.Offset}}), 
                    UNNEST({{Parameters.Event.EventType}}), 
                    UNNEST({{Parameters.Event.CapturedAt}}), 
                    UNNEST({{Parameters.Event.CapturedBy}}), 
                    UNNEST({{Parameters.Event.TelemetryContext}}), 
                    UNNEST({{Parameters.Event.Payload}})
            """,
            SaveToOutbox = $$"""
             INSERT INTO {{tblInitial}}{0}
                    ({{Fields.Message.Id}}, 
                    {{Fields.Message.Domain}}, 
                    {{Fields.Message.Partition}}, 
                    {{Fields.Message.StreamId}}, 
                    "{{Fields.Message.Offset}}", 
                    {{Fields.Message.Channel}}, 
                    {{Fields.Message.MessageType}}, 
                    {{Fields.Message.SerializeType}}, 
                    {{Fields.Message.EventType}}, 
                    {{Fields.Message.CapturedAt}}, 
                    {{Fields.Message.CapturedBy}}, 
                    {{Fields.Message.TelemetryContext}}, 
                    {{Fields.Message.Payload}})
                SELECT 
                    UNNEST({{Parameters.Message.Id}}), 
                    UNNEST({{Parameters.Message.Domain}}), 
                    UNNEST({{Parameters.Message.Partition}}), 
                    UNNEST({{Parameters.Message.StreamId}}), 
                    UNNEST({{Parameters.Message.Offset}}), 
                    UNNEST({{Parameters.Message.Channel}}), 
                    UNNEST({{Parameters.Message.MessageType}}), 
                    UNNEST({{Parameters.Message.SerializeType}}), 
                    UNNEST({{Parameters.Message.EventType}}), 
                    UNNEST({{Parameters.Message.CapturedAt}}), 
                    UNNEST({{Parameters.Message.CapturedBy}}), 
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
                WHERE {Fields.Snapshot.Domain} = {Parameters.Snapshot.Domain}
                    AND {Fields.Snapshot.Partition} = {Parameters.Snapshot.Partition}
                    AND {Fields.Snapshot.StreamId} = {Parameters.Snapshot.StreamId}
                    AND {Fields.Snapshot.ViewName} = {Parameters.Snapshot.ViewName}
                ORDER BY "{Fields.Snapshot.Offset}" DESC
                LIMIT 1;
                """,
            SaveSnapshot = $"""
            INSERT INTO {tabInitial}snapshot (
                        {Fields.Snapshot.Id},
                        {Fields.Snapshot.Domain},
                        {Fields.Snapshot.Partition},
                        {Fields.Snapshot.StreamId},
                        {Fields.Snapshot.ViewName},
                        "{Fields.Snapshot.Offset}",
                        {Fields.Snapshot.State})
            VALUES (
                        {Parameters.Snapshot.Id},
                        {Parameters.Snapshot.Domain},
                        {Parameters.Snapshot.Partition},
                        {Parameters.Snapshot.StreamId},
                        {Parameters.Snapshot.ViewName},
                        {Parameters.Snapshot.Offset},
                        {Parameters.Snapshot.State})
            """
        };
    }
}
