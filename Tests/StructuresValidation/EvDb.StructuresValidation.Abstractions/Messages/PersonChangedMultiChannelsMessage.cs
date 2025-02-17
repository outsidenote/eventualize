using EvDb.Core;

namespace EvDb.StructuresValidation.Abstractions;

[EvDbAttachDefaultChannel]
[EvDbAttachChannel(OutboxChannels02.Channel2)]
[EvDbAttachChannel(OutboxChannels02.Channel1)]
[EvDbDefineMessagePayload("email-validated-status-03")]
public readonly partial record struct PersonChangedMultiChannelsMessage()
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public bool EmailIsValid { get; init; }
}


