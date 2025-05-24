using EvDb.Core;
using EvDb.Core.Adapters;

namespace EvDb.UnitTests;

public static class Helpers
{
    public static void AssertTelemetryContextEquals(
        this EvDbMessageRecord messageRecord,
        EvDbTelemetryContextName expected)
    {
        messageRecord.TelemetryContext.AssertJsonEquals(expected);
    }

    public static void AssertTelemetryContextEquals(
        this EvDbMessageRecord messageRecord,
        EvDbMessageRecord expected)
    {
        messageRecord.TelemetryContext.AssertJsonEquals(expected.TelemetryContext);
    }

    public static void AssertTelemetryContextEquals(
        this EvDbMessageRecord messageRecord,
        IEvDbMessageMeta expected)
    {
        messageRecord.TelemetryContext.AssertJsonEquals(expected.TelemetryContext);
    }

    public static void AssertTelemetryContextEquals(
        this IEvDbMessageMeta messageRecord,
        EvDbTelemetryContextName expected)
    {
        messageRecord.TelemetryContext.AssertJsonEquals(expected);
    }

    public static void AssertJsonEquals(
        this EvDbTelemetryContextName otelContext,
        EvDbTelemetryContextName expected)
    {
        bool isEquals = otelContext.JsonEquals(expected);
        Assert.True(isEquals);
    }
}
