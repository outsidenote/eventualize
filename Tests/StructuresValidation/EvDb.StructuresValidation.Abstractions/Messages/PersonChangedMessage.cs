using EvDb.Core;

namespace EvDb.StructuresValidation.Abstractions.Events;

[EvDbDefineMessagePayload("email-validated-status")]
public readonly partial record struct PersonChangedMessage()
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public bool EmailIsValid { get; init; }
}


