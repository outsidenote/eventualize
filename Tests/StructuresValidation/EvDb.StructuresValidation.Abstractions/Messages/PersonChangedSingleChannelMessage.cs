using EvDb.Core;

namespace EvDb.StructuresValidation.Abstractions;

[EvDbAttachChannel(OutboxChannels02.Channel1)]
[EvDbDefineMessagePayload("email-validated-status-single-channel")]
public readonly partial record struct PersonChangedSingleChannelMessage()
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public bool EmailIsValid { get; init; }
}


