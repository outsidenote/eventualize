using EvDb.Core;
using System.Diagnostics.Metrics;
using static EvDb.Core.Internals.OtelConstants;

namespace EvDb.Sinks;

// https://www.mytechramblings.com/posts/getting-started-with-opentelemetry-metrics-and-dotnet-part-1/

public abstract class EvDbSinkMeters : IEvDbSinkMeters
{
    private readonly Counter<int> _published;

    public string Name { get; }

    public EvDbSinkMeters(string sinkChannel) : this(new Meter($"EvDb.Sink.{sinkChannel}"), sinkChannel)
    {
    }

    public EvDbSinkMeters(Meter meter, string sinkChannel)
    {
        _published = meter.CreateCounter<int>($"evdb_{sinkChannel}_published",
            "{messages}",
            $"Number of publish via {sinkChannel}");

        Name = meter.Name;
    }

    void IEvDbSinkMeters.IncrementPublish(EvDbSinkTarget target)
    {
        var tags = OtelTags.Create(TAG_SINK_TARGET_NAME, target.ToString());
        _published.Add(1, tags);
    }
}
