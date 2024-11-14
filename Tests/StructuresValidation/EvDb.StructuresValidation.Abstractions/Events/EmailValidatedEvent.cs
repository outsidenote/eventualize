using EvDb.Core;

namespace EvDb.StructuresValidation.Abstractions.Events;

[EvDbDefineEventPayload("email-validated")]
public readonly partial record struct EmailValidatedEvent(string Email, bool IsValid);


