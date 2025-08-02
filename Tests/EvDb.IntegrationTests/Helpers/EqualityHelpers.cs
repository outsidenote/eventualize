using EvDb.Core.Adapters;
using System.Diagnostics;

namespace EvDb.Core.Tests;

public static class EqualityHelpers
{
    public static void AssertTelemetryContextEquals(
        this EvDbMessageRecord messageRecord,
        Activity? expected)
    {
        EvDbOtelTraceParent? traceParent = expected?.SerializeTelemetryContext();
        Assert.Equal(messageRecord.TraceParent, traceParent);
    }   
}
