using EvDb.Core;
using EvDb.Core.Adapters;

namespace EvDb.UnitTests;

public static class Helpers
{
    public static void AssertTelemetryContextEquals(
        this EvDbMessageRecord messageRecord,
        EvDbOtelTraceParent expected)
    {
        messageRecord.TraceParent.AssertTraceParentEquals(expected);
    }

    public static void AssertTelemetryContextEquals(
        this EvDbMessageRecord messageRecord,
        EvDbMessageRecord expected)
    {
        messageRecord.TraceParent.AssertTraceParentEquals(expected.TraceParent);
    }

    public static void AssertTelemetryContextEquals(
        this EvDbMessageRecord messageRecord,
        IEvDbMessageMeta expected)
    {
        messageRecord.TraceParent.AssertTraceParentEquals(expected.TraceParent);
    }

    public static void AssertTelemetryContextEquals(
        this IEvDbMessageMeta messageRecord,
        EvDbOtelTraceParent expected)
    {
        messageRecord.TraceParent.AssertTraceParentEquals(expected);
    }

    public static void AssertTraceParentEquals(
        this EvDbOtelTraceParent traceParent,
        EvDbOtelTraceParent expected)
    {
        bool isEquals = traceParent == expected;
        Assert.True(isEquals);
    }
}
