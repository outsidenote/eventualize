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

        propagator = propagator ?? Propagator;

        // Use ArrayBufferWriter from System.Buffers for better memory efficiency
        ArrayBufferWriter<byte> bufferWriter = new();
        using var writer = new Utf8JsonWriter(bufferWriter);

        Baggage baggage = Baggage.Current;
        if (activity.Baggage.Any())
        {
            Dictionary<string, string> baggageItems = activity.Baggage
                                                        .Select(kvp => KeyValuePair.Create(kvp.Key, kvp.Value ?? string.Empty))
                                                        .ToDictionary();
            baggage = Baggage.Create(baggageItems);
        }

        writer.WriteStartObject();

        // Inject context directly
        propagator.Inject(
            new PropagationContext(activity.Context, baggage),
            writer,
            (w, key, value) => w.WriteString(key, value));

        writer.WriteEndObject();
        writer.Flush();

        // Return the written data as a byte array
        var span = bufferWriter.WrittenSpan;
        var result = EvDbOtelTraceParent.FromSpan(span);
        return result;
    }

    #endregion //  SerializeTelemetryContext

    #region ToTelemetryContext

    /// <summary>
    /// Extract EvDbOtelTraceParent into OTEL context
    /// </summary>
    /// <param name="contextData"></param>
    /// <param name="propagator"></param>
    /// <returns></returns>
    public static ActivityContext ToTelemetryContext(this EvDbOtelTraceParent contextData, TextMapPropagator? propagator = null)
    {
        if (contextData == EvDbOtelTraceParent.Empty || string.IsNullOrEmpty(contextData))
            return default;

        return ActivityContext.TryParse(
            contextData,
            null, // No trace state
            out ActivityContext activityContext) ? 
            activityContext : 
            default;
    }

    #endregion //  ToTelemetryContex
}

