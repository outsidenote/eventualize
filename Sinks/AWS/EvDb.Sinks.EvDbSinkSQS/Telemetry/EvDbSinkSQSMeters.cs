// Ignore Spelling: SQS
#pragma warning disable S101 // Types should be named in PascalCase

using System.Diagnostics.Metrics;

namespace EvDb.Sinks.EvDbSinkSQS;

// https://www.mytechramblings.com/posts/getting-started-with-opentelemetry-metrics-and-dotnet-part-1/

public class EvDbSinkSQSMeters : EvDbSinkMeters, IEvDbSinkSQSMeters
{
    public static readonly IEvDbSinkSQSMeters Default = new EvDbSinkSQSMeters();

    private const string SINK_CHANNEL = "sqs";

    public EvDbSinkSQSMeters() : base(SINK_CHANNEL)
    {
    }

    public EvDbSinkSQSMeters(Meter meter) : base(meter, SINK_CHANNEL)
    {
    }
}
