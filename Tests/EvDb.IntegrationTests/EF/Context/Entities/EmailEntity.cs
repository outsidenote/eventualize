namespace EvDb.IntegrationTests.EF;

public record EmailEntity(string Value, string Domain, string Category)
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public int PersonId { get; init; }
}
