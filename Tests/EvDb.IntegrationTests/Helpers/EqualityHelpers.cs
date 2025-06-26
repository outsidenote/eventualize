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
            expected?.SerializeTelemetryContext() ?? EvDbTelemetryContextName.Empty);
    }
    public static void AssertTelemetryContextEquals(
        this EvDbMessageRecord messageRecord,
        EvDbTelemetryContextName expected)
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
        EvDbTelemetryContextName expected)
    {
        EvDbTelemetryContextName value = activity?.SerializeTelemetryContext() ?? EvDbTelemetryContextName.Empty;
        value.AssertJsonEquals(expected);
    }

    public static void AssertJsonEquals(
        this EvDbTelemetryContextName otelContext,
        EvDbTelemetryContextName expected)
    {
        bool isEquals = otelContext.JsonEquals(expected);
        Assert.True(isEquals, "OTEL Equality");
    }
}
