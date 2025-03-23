namespace MongoBenchmark;

public readonly record struct EvDbEvent(string EventType,
                                          DateTimeOffset CapturedAt,
                                          string CapturedBy,
                                          EvDbStreamCursor StreamCursor,
                                          byte[] Payload);
