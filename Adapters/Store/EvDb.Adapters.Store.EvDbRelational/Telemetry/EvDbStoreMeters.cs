using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using EvDb.Core;

namespace EvDb.Core.Adapters;

// https://www.mytechramblings.com/posts/getting-started-with-opentelemetry-metrics-and-dotnet-part-1/

internal class EvDbStoreMeters : IEvDbStoreMeters
{
    public const string MetricName = "EvDb.Store.Relational";

    public EvDbStoreMeters() : this(
        new Meter(MetricName))
    {
    }

    public EvDbStoreMeters([FromKeyedServices(MetricName)] IMeterFactory meterFactory)
                                : this(meterFactory.Create(MetricName))
    {
    }

    private EvDbStoreMeters(Meter counterMeter)
    {
        _eventsStored = counterMeter.CreateCounter<int>("evdb_store_events_stored",
            "{messages}",
            "Count of messages stored into the storage");
        _messagesStored = counterMeter.CreateCounter<int>("evdb_store_notification_stored",
            "{messages}",
            "Count of messages stored into topics ");
    }

    /// <summary>
    /// Number of messages stored
    /// </summary>
    private readonly Counter<int> _eventsStored;

    /// <summary>
    /// Number of Notification stored (into topics)
    /// </summary>
    private readonly Counter<int> _messagesStored;

    void IEvDbStoreMeters.AddEvents(int count, IEvDbStreamStoreData streamStore, string dbType)
    {
        if (!_eventsStored.Enabled)
            return;

        var adr = streamStore.StreamAddress;
        _eventsStored.Add(count, tags => tags.Add("evdb_store_db", dbType)
                                                          .Add("evdb_store_domain", adr.Domain)
                                                          .Add("evdb_store_partition", adr.Partition));
    }

    void IEvDbStoreMeters.AddMessages(int count,
                                      IEvDbStreamStoreData streamStore,
                                      string dbType,
                                      EvDbTableName tableName)
    {
        if (!_messagesStored.Enabled)
            return;
        var adr = streamStore.StreamAddress;

        _eventsStored.Add(count, tags => tags.Add("evdb_store_db", dbType)
                                                      .Add("evdb_store_domain", adr.Domain)
                                                      .Add("evdb_store_partition", adr.Partition)
                                                      .Add("evdb_store_topic_table", tableName)                                                      );
    }
}
