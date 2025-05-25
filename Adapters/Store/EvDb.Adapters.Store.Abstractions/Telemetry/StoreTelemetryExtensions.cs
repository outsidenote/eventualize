using EvDb.Core.Adapters;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Buffers;
using System.Diagnostics;
using System.Text.Json;

namespace EvDb.Core;

public static class StoreTelemetryExtensions
{
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    #region SerializeTelemetryContext

    /// <summary>
    /// Extract context from Activity into Json
    /// </summary>
    /// <param name="activity"></param>
    /// <param name="propagator"></param>
    /// <returns></returns>
    public static EvDbTelemetryContextName SerializeTelemetryContext(this Activity activity, TextMapPropagator? propagator = null)
    {
        if (activity.Context == default)
            return EvDbTelemetryContextName.Empty;

        propagator = propagator ?? Propagator;

        // Use ArrayBufferWriter from System.Buffers for better memory efficiency
        ArrayBufferWriter<byte> bufferWriter = new ();
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
        var result = EvDbTelemetryContextName.FromSpan(span);
        return result;
    }

    #endregion //  SerializeTelemetryContext

    #region ToTelemetryContext

    /// <summary>
    /// Extract EvDbTelemetryContextName into OTEL context
    /// </summary>
    /// <param name="contextData"></param>
    /// <param name="propagator"></param>
    /// <returns></returns>
    public static ActivityContext ToTelemetryContext(this EvDbTelemetryContextName contextData, TextMapPropagator? propagator = null)
    {
        if (contextData == EvDbTelemetryContextName.Empty || contextData.Length == 0)
            return default;

        propagator = propagator ?? Propagator;

        // Use Utf8JsonReader directly with the span
        var reader = new Utf8JsonReader(contextData);



        // Use a local function for the extraction delegate to improve readability and performance
        static IEnumerable<string>? ExtractValue(JsonElement carrier, string key)
        {
            // Fast path check
            if (!carrier.TryGetProperty(key, out var prop) || prop.ValueKind != JsonValueKind.String)
                return null;

            string? value = prop.GetString();
            return string.IsNullOrEmpty(value) ? null : new[] { value };
        }

        var json = contextData.ToJson();

        // Extract the propagation context with the optimized delegate
        var propagationContext = propagator.Extract(default, json, ExtractValue);

        return propagationContext.ActivityContext;
    }

    #endregion //  ToTelemetryContext

    // TODO: [bnaya 2025-05-21] start Activity

    #region AddEvDbStoreInstrumentation

    public static TracerProviderBuilder AddEvDbStoreInstrumentation(this TracerProviderBuilder builder)
    {
        builder.AddSource(StoreTelemetry.TraceName);
        return builder;
    }

    #endregion //  AddEvDbStoreInstrumentation

    #region AddEvDbStoreInstrumentation

    public static MeterProviderBuilder AddEvDbStoreInstrumentation(this MeterProviderBuilder builder,
                                                              EvDbMeters include = EvDbMeters.All)
    {
        builder.AddMeter(EvDbStoreMeters.MetricName);
        return builder;
    }

    #endregion //  AddEvDbStoreInstrumentation
}

