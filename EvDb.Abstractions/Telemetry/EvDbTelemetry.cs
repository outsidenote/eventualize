using System.Diagnostics;

namespace EvDb.Core;

public abstract class EvDbTelemetry
{
    public const string TraceName = "EvDb";

    public static ActivitySource Trace { get; } = new ActivitySource(TraceName);
}

