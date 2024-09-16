namespace EvDb.Core;

public interface IEvDbEventMeta
{
    EvDbStreamCursor StreamCursor { get; }
    string EventType { get; }
    DateTimeOffset CapturedAt { get; }
    string CapturedBy { get; }
}

public interface IEvDbOutboxMeta: IEvDbEventMeta
{
    string OutboxType { get; }
}
