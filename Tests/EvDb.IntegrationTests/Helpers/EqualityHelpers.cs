using EvDb.Core.Adapters;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace EvDb.Core.Tests;

public static class EqualityHelpers
{
    public static void AssertTelemetryContextEquals(
        this EvDbMessageRecord messageRecord,
        Activity? expected)
    {
        messageRecord.TelemetryContext.AssertJsonEquals(expected?.SerializeTelemetryContext());
    }
    public static void AssertTelemetryContextEquals(
        this EvDbMessageRecord messageRecord,
        byte[]? expected)
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
        byte[]? expected)
    {
        Assert.True(activity == null && expected == null || activity != null && expected != null);
        activity?.SerializeTelemetryContext().AssertJsonEquals(expected);
    }

    public static void AssertJsonEquals(
        this byte[]? otelContext,
        byte[]? expected)
    {
        Assert.True(otelContext == null && expected == null || otelContext != null && expected != null);
        if (otelContext == null || expected == null)
            return;

        var token1 = JsonDocument.Parse(Encoding.UTF8.GetString(otelContext));
        var token2 = JsonDocument.Parse(Encoding.UTF8.GetString(expected));
        string ser1 = JsonSerializer.Serialize(token1);
        string ser2 = JsonSerializer.Serialize(token2);
        Assert.Equal(ser2, ser1);
    }
}
