// Ignore Spelling: SNS
#pragma warning disable S101 // Types should be named in PascalCase

using System.Diagnostics.Metrics;

namespace EvDb.Sinks.EvDbSinkSNS;

// https://www.mytechramblings.com/posts/getting-started-with-opentelemetry-metrics-and-dotnet-part-1/

public class EvDbSinkSNSMeters : EvDbSinkMeters, IEvDbSinkSNSMeters
{
    public static readonly IEvDbSinkSNSMeters Default = new EvDbSinkSNSMeters(SINK_CHANNEL);

    private const string SINK_CHANNEL = "sns";

    public EvDbSinkSNSMeters(string sinkChannel) : base(SINK_CHANNEL)
    {
    }

    public EvDbSinkSNSMeters(Meter meter, string sinkChannel) : base(meter, SINK_CHANNEL)
    {
    }
}
