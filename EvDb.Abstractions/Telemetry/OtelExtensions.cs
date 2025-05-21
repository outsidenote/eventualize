using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Buffers;
using System.Diagnostics;
using System.Text.Json;

namespace EvDb.Core;

public static class OtelExtensions
{
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    #region SerializeTelemetryContext

    /// <summary>
    /// Extract context from Activity into Json
    /// </summary>
    /// <param name="activity"></param>
    /// <returns></returns>
    public static byte[]? SerializeTelemetryContext(this Activity? activity)
    {
        if (activity == null || activity.Context == default)
            return null;

        // Use ArrayBufferWriter from System.Buffers for better memory efficiency
        var bufferWriter = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(bufferWriter);

        writer.WriteStartObject();

        // Inject context directly
        Propagator.Inject(
            new PropagationContext(activity.Context, Baggage.Current),
            writer,
            (w, key, value) => w.WriteString(key, value));

        writer.WriteEndObject();
        writer.Flush();

        // Return the written data as a byte array
        return bufferWriter.WrittenSpan.ToArray();
    }

    #endregion //  SerializeTelemetryContext

    #region ToTelemetryContext

    /// <summary>
    /// Extract byte[] into OTEL context
    /// </summary>
    /// <param name="contextData"></param>
    /// <returns></returns>
    public static ActivityContext ToTelemetryContext(byte[]? contextData)
    {
        if (contextData == null || contextData.Length == 0)
            return default;

        // Use Utf8JsonReader directly with the span
        var reader = new Utf8JsonReader(contextData);

        // Try to parse and extract in one go
        if (!JsonDocument.TryParseValue(ref reader, out var doc))
            return default;

        // Use a local function for the extraction delegate to improve readability and performance
        static IEnumerable<string>? ExtractValue(JsonElement carrier, string key)
        {
            // Fast path check
            if (!carrier.TryGetProperty(key, out var prop) || prop.ValueKind != JsonValueKind.String)
                return null;

            string? value = prop.GetString();
            return string.IsNullOrEmpty(value) ? null : new[] { value };
        }

        // Extract the propagation context with the optimized delegate
        var propagationContext = Propagator.Extract(default, doc.RootElement, ExtractValue);

        return propagationContext.ActivityContext;
    }

    #endregion //  ToTelemetryContext

    // TODO: [bnaya 2025-05-21] start Activity
}
