namespace EvDb.StructuresValidation.Abstractions.Views;

public readonly record struct Person(Guid Id)
{
    public string Email { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public bool EmailIsValid { get; init; }
}