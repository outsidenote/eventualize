using EvDb.Core;

namespace EvDb.StructuresValidation.Abstractions;

[EvDbAttachDefaultChannel]
[EvDbDefineMessagePayload("email-validated-status-default-channel")]
public readonly partial record struct PersonChangedDefaultChannelMessage()
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public bool EmailIsValid { get; init; }
}


