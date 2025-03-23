using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.Metrics;

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
            "Count of messages stored into outbox");
    }

    /// <summary>
    /// Number of messages stored
    /// </summary>
    private readonly Counter<int> _eventsStored;

    /// <summary>
    /// Number of Notification stored (into outbox)
    /// </summary>
    private readonly Counter<int> _messagesStored;

    void IEvDbStoreMeters.AddEvents(
        int count,
        EvDbStreamAddress address,
        string dbType)
    {
        if (!_eventsStored.Enabled)
            return;

        _eventsStored.Add(count, tags => tags.Add("evdb_store_db", dbType)
                                                          .Add("evdb_store_domain", address.Domain)
                                                          .Add("evdb_store_partition", address.Partition));
    }

    void IEvDbStoreMeters.AddMessages(int count,
                                      EvDbStreamAddress address,
                                      string dbType,
                                      EvDbShardName shardName)
    {
        if (!_messagesStored.Enabled)
            return;

        _eventsStored.Add(count, tags => tags.Add("evdb_store_db", dbType)
                                                      .Add("evdb_store_domain", address.Domain)
                                                      .Add("evdb_store_partition", address.Partition)
                                                      .Add("evdb_store_shard", shardName));
    }
}
