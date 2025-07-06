// Ignore Spelling: SQS
#pragma warning disable S101 // Types should be named in PascalCase

using System.Diagnostics.Metrics;

namespace EvDb.Sinks.EvDbSinkKafka;

// https://www.mytechramblings.com/posts/getting-started-with-opentelemetry-metrics-and-dotnet-part-1/

public class EvDbSinkKafkaMeters : EvDbSinkMeters, IEvDbSinkKafkaMeters
{
    public static readonly IEvDbSinkKafkaMeters Default = new EvDbSinkKafkaMeters();

    private const string SINK_CHANNEL = "sqs";

    public EvDbSinkKafkaMeters() : base(SINK_CHANNEL)
    {
    }

    public EvDbSinkKafkaMeters(Meter meter) : base(meter, SINK_CHANNEL)
    {
    }
}
