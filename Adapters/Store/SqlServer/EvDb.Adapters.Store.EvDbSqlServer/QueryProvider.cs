using EvDb.Core;
using EvDb.Core.Adapters;
using static EvDb.Core.Adapters.Internals.EvDbStoreNames;

namespace EvDb.Adapters.Store.SqlServer;

internal static class QueryProvider
{
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
                    {Fields.Event.Offset} as {Projection.Event.Offset}           
                FROM {tblInitial}events WITH (READCOMMITTEDLOCK)
                WHERE {Parameters.Event.StreamType} = {Parameters.Event.StreamType}
                    AND {Parameters.Event.StreamId} = {Parameters.Event.StreamId}
                Order BY {Fields.Event.Offset} DESC;
            """,
            GetEvents = $"""
                SELECT
                    {Fields.Event.StreamType} as {Projection.Event.StreamType},
                    {Fields.Event.StreamId} as {Projection.Event.StreamId},
                    {Fields.Event.Offset} as {Projection.Event.Offset},
                    {Fields.Event.EventType} as {Projection.Event.EventType},
                    {Fields.Event.CapturedAt} as {Projection.Event.CapturedAt},
                    {Fields.Event.CapturedBy} as {Projection.Event.CapturedBy},
                    {Fields.Event.TelemetryContext} as {Projection.Event.TelemetryContext},
                    {Fields.Event.Payload} as {Projection.Event.Payload}                  
                FROM {tblInitial}events WITH (READCOMMITTEDLOCK)
                WHERE {Fields.Event.StreamType} = {Parameters.Event.StreamType}
                    AND {Fields.Event.StreamId} = {Parameters.Event.StreamId}
                    AND {Fields.Event.Offset} >= {Parameters.Event.SinceOffset};
                """,
            GetMessages = $$"""
                SELECT
                    {{Fields.Message.Id}} as {{Projection.Message.Id}},
                    {{Fields.Message.StreamType}} as {{Projection.Message.StreamType}},
                    {{Fields.Message.StreamId}} as {{Projection.Message.StreamId}},
                    {{Fields.Message.Offset}} as {{Projection.Message.Offset}},
                    {{Fields.Message.EventType}} as {{Projection.Message.EventType}},
                    {{Fields.Message.MessageType}} as {{Projection.Message.MessageType}},
                    {{Fields.Message.CapturedAt}} as {{Projection.Message.CapturedAt}},
                    {{Fields.Message.StoredAt}} as {{Projection.Message.StoredAt}},
                    {{Fields.Message.CapturedBy}} as {{Projection.Message.CapturedBy}},
                    {{Fields.Message.Channel}} as {{Projection.Message.Channel}},
                    {{Fields.Message.SerializeType}} as {{Projection.Message.SerializeType}},
                    {{Fields.Message.TelemetryContext}} as {{Projection.Message.TelemetryContext}},
                    {{Fields.Message.Payload}} as {{Projection.Message.Payload}}                  
                FROM {{tblInitial}}{0} WITH (READCOMMITTEDLOCK)                
                WHERE 
                    {{Fields.Message.StoredAt}} >= {{Parameters.Message.SinceDate}}
                    AND {{Fields.Message.StoredAt}} < DATEADD(MICROSECOND, -1000, SYSDATETIME())
                    AND (
                        {{Parameters.Message.Channels}} IS NULL 
                        OR ISJSON({{Parameters.Message.Channels}}) = 0 
                        OR {{Parameters.Message.Channels}} = '[]'
                        OR EXISTS (SELECT 1 FROM OPENJSON({{Parameters.Message.Channels}}) WHERE value = {{Fields.Message.Channel}})
                    )
                    AND (
                        {{Parameters.Message.MessageTypes}} IS NULL 
                        OR ISJSON({{Parameters.Message.MessageTypes}}) = 0 
                        OR {{Parameters.Message.MessageTypes}} = '[]'
                        OR EXISTS (SELECT 1 FROM OPENJSON({{Parameters.Message.MessageTypes}}) WHERE value = {{Fields.Message.MessageType}})
                    )                         
                ORDER BY {{Fields.Message.StoredAt}} ASC, {{Fields.Message.Channel}} ASC, {{Fields.Message.MessageType}} ASC, {{Fields.Event.Offset}} ASC;
                """,
            // take a look at https://www.learndapper.com/saving-data/insert
            SaveEvents = $"{tblInitial}InsertEventsBatch_Events",
            SaveToOutbox = $$"""{{tblInitial}}InsertOutboxBatch_{0}"""
        };
    }

    public static EvDbSnapshotAdapterQueryTemplates CreateSnapshotQueries(EvDbStorageContext storageContext)
    {
        string tabInitial = storageContext.Id;

        return new EvDbSnapshotAdapterQueryTemplates
        {
            GetSnapshot = $"""
                SELECT {Fields.Snapshot.State} as {Projection.Snapshot.State}, 
                        {Fields.Snapshot.Offset} as {Projection.Snapshot.Offset}
                FROM {tabInitial}snapshot
                WHERE {Fields.Snapshot.StreamType} = {Parameters.Snapshot.StreamType}
                    AND {Fields.Snapshot.StreamId} = {Parameters.Snapshot.StreamId}
                    AND {Fields.Snapshot.ViewName} = {Parameters.Snapshot.ViewName}
                ORDER BY offset DESC
                OFFSET 0 ROWS FETCH FIRST 1 ROWS ONLY;
                """,
            SaveSnapshot = $"""
            INSERT INTO {tabInitial}snapshot (
                        {Fields.Snapshot.Id},
                        {Fields.Snapshot.StreamType},
                        {Fields.Snapshot.StreamId},
                        {Fields.Snapshot.ViewName},
                        {Fields.Snapshot.Offset},
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
