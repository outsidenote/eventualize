using System.Diagnostics.Metrics;

namespace EvDb.Sinks;

// https://www.mytechramblings.com/posts/getting-started-with-opentelemetry-metrics-and-dotnet-part-1/

public abstract class EvDbSinkMeters : IEvDbSinkMeters
{
    public string Name { get; }

    public EvDbSinkMeters(string sinkChannel) : this(new Meter($"EvDb.Sink.{sinkChannel}"), sinkChannel)
    {
    }

    public EvDbSinkMeters(Meter meter, string sinkChannel)
    {
        Published = meter.CreateCounter<int>($"evdb_{sinkChannel}_published",
            "{messages}",
            $"Number of publish via {sinkChannel}");

        Name = meter.Name;
    }

    /// <summary>
    /// Optimistic Concurrency Collisions
    /// </summary>
    public Counter<int> Published { get; }
}
