using EvDb.Core.Adapters;
using System.Diagnostics;

namespace EvDb.Core.Tests;

public static class EqualityHelpers
{
    public static void AssertTelemetryContextEquals(
        this EvDbMessageRecord messageRecord,
        Activity? expected)
    {
        messageRecord.TelemetryContext.AssertJsonEquals(
            expected?.SerializeTelemetryContext() ?? EvDbOtelTraceParent.Empty);
    }
    public static void AssertTelemetryContextEquals(
        this EvDbMessageRecord messageRecord,
        EvDbOtelTraceParent expected)
    {
        messageRecord.TelemetryContext.AssertJsonEquals(expected);
    }

    public static void AssertOtelEqualsContext(
        this EvDbMessageRecord messageRecord,
        EvDbMessageRecord expected)
    {
        messageRecord.TelemetryContext.AssertJsonEquals(expected.TelemetryContext);
    }

    public static void AssertTelemetryContextEquals(
        this Activity? activity,
        EvDbOtelTraceParent expected)
    {
        EvDbOtelTraceParent value = activity?.SerializeTelemetryContext() ?? EvDbOtelTraceParent.Empty;
        value.AssertJsonEquals(expected);
    }

    public static void AssertJsonEquals(
        this EvDbOtelTraceParent otelContext,
        EvDbOtelTraceParent expected)
    {
        bool isEquals = otelContext.JsonEquals(expected);
        Assert.True(isEquals, "OTEL Equality");
    }
}
