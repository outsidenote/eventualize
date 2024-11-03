using EvDb.Core;

namespace EvDb.StructuresValidation.Abstractions.Events;

[EvDbDefinePayload("email-validated")]
public readonly partial record struct EmailValidatedEvent(string Email, bool IsValid);


