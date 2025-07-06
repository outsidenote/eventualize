
// Wrapper type to allow generic serialization/deserialization
public record MassTransitEnvelope
{
    public string Payload { get; init; } = default!;
    public string Target { get; init; } = default!;
    public Guid MessageId { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}
