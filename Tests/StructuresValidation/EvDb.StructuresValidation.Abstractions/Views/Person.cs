namespace EvDb.StructuresValidation.Abstractions.Views;

public readonly record struct Person (Guid Id)
{
    // public static readonly Person Empty = new(Guid.Empty);

    public string Email { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public bool EmailIsValid { get; init; }
}