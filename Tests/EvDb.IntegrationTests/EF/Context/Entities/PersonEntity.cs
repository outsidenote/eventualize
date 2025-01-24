namespace EvDb.IntegrationTests.EF;

public record PersonEntity
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public DateOnly Birthday { get; init; }
    public List<EmailEntity> Emails { get; init; } = new List<EmailEntity>();
    public required AddressEntity? Address { get; init; }
}
