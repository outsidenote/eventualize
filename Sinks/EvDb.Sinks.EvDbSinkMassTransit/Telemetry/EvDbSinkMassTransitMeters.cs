// Ignore Spelling: SNS
#pragma warning disable S101 // Types should be named in PascalCase

using System.Diagnostics.Metrics;

namespace EvDb.Sinks.EvDbSinkMassTransit;

// https://www.mytechramblings.com/posts/getting-started-with-opentelemetry-metrics-and-dotnet-part-1/

public class EvDbSinkMassTransitMeters : EvDbSinkMeters, IEvDbSinkMassTransitMeters
{
    public static readonly IEvDbSinkMassTransitMeters Default = new EvDbSinkMassTransitMeters();

    private const string SINK_CHANNEL = "sns";

    public EvDbSinkMassTransitMeters() : base(SINK_CHANNEL)
    {
    }

    public EvDbSinkMassTransitMeters(Meter meter) : base(meter, SINK_CHANNEL)
    {
    }
}
