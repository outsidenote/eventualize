namespace EvDb.Core;

public interface IEvDbEventMeta
{
    /// <summary>
    /// The full address of the stream including the offset
    /// </summary>
    EvDbStreamCursor StreamCursor { get; }

    /// <summary>
    /// The type of the event
    /// </summary>
    string EventType { get; }

    /// <summary>
    /// The time of capturing the event (client side time)
    /// </summary>
    DateTimeOffset CapturedAt { get; }

    /// <summary>
    /// The user that captured the event
    /// </summary>
    string CapturedBy { get; }

    /// <summary>
    /// Json format of the Trace (Open Telemetry) propagated context at the persistent time.
    /// The value will be null if the Trace is null when persisting the record or before persistent.
    /// </summary>
    public EvDbTelemetryContextName TelemetryContext { get; init; }
}
