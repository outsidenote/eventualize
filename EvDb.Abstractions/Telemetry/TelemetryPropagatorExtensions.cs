using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Buffers;
using System.Diagnostics;
using System.Text.Json;

namespace EvDb.Core;

public static class TelemetryPropagatorExtensions
{
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    #region SerializeTelemetryContext

    /// <summary>
    /// Extract context from Activity into Json
    /// </summary>
    /// <param name="activity"></param>
    /// <param name="propagator"></param>
    /// <returns></returns>
    public static EvDbOtelTraceParent SerializeTelemetryContext(this Activity activity, TextMapPropagator? propagator = null)
    {
        if (activity.Context == default)
            return EvDbOtelTraceParent.Empty;

        if (activity.Context.TraceId == default)
            return EvDbOtelTraceParent.Empty;
        if (activity.Context.SpanId == default)
            return EvDbOtelTraceParent.Empty;

        string flags = activity.Context.TraceFlags.HasFlag(ActivityTraceFlags.Recorded) ? "01" : "00";
        string trace = activity.Context.TraceId.ToHexString();
        string span = activity.Context.SpanId.ToHexString();

        // 00-<trace-id>-<span-id>-<trace-flags> 
        // flags: 0x01 = sampled, 0x00 = not sampled
        EvDbOtelTraceParent traceParent = $"00-{trace}-{span}-{flags}";
        return traceParent;
    }

    #endregion //  SerializeTelemetryContext

    #region ToTelemetryContext

    /// <summary>
    /// Extract EvDbOtelTraceParent into OTEL context
    /// </summary>
    /// <param name="traceParent"></param>
    /// <param name="propagator"></param>
    /// <returns></returns>
    public static ActivityContext ToTelemetryContext(this EvDbOtelTraceParent traceParent, TextMapPropagator? propagator = null)
    {
        if (traceParent == EvDbOtelTraceParent.Empty || string.IsNullOrEmpty(traceParent))
            return default;

        return ActivityContext.TryParse(
            traceParent,
            null, // No trace state
            out ActivityContext activityContext) ? 
            activityContext : 
            default;
    }

    #endregion //  ToTelemetryContex
}

