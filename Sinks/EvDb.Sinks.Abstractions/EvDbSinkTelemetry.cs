using System.Diagnostics;

namespace EvDb.Sinks;

public static class EvDbSinkTelemetry
{
    public const string TraceName = "EvDb:Sink";

    public static ActivitySource OtelTrace { get; } = new ActivitySource(TraceName);
}

