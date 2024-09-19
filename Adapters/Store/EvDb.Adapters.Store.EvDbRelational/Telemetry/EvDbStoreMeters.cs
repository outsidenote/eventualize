using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using System.Diagnostics;
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
            "{events}",
            "Count of events stored into the storage");
        _messagesStored = counterMeter.CreateCounter<int>("evdb_store_notification_stored",
            "{events}",
            "Count of messages stored into topics ");
    }

    /// <summary>
    /// Number of events stored
    /// </summary>
    private readonly Counter<int> _eventsStored;

    /// <summary>
    /// Number of Notification stored (into topics)
    /// </summary>
    private readonly Counter<int> _messagesStored;

    void IEvDbStoreMeters.AddEvents(IImmutableList<EvDbEvent> events)
    {
        if (!_eventsStored.Enabled)
            return;
        // TODO: bnaya 2024-09-19 group by
    }

    void IEvDbStoreMeters.AddMessages(IImmutableList<EvDbMessage> events)
    {
        if (!_messagesStored.Enabled)
            return;
        // TODO: bnaya 2024-09-19 group by
    }
}
