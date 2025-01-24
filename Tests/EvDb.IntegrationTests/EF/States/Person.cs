namespace EvDb.IntegrationTests.EF.States;
public readonly record struct Person(int Id,
                                     string Name,
                                     DateOnly Birthday,
                                     Address? Address = null)
{
    public Email[] Emails { get; init; } = Array.Empty<Email>();
}
