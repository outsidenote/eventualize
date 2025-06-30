namespace EvDb.Sinks;

public readonly record struct EvDbSinkCheckpoint(
    string SinkName,
    string ConsumerGroup,
    DateTimeOffset Checkpoint)
{
    public static readonly EvDbSinkCheckpoint Empty = new EvDbSinkCheckpoint(string.Empty, string.Empty, DateTimeOffset.MinValue);

    public bool IsEmpty => this == Empty;

    public override string ToString() => $"{SinkName}/{ConsumerGroup} @ {Checkpoint}";
}
