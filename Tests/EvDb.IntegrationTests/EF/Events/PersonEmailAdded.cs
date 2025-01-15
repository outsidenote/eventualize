using EvDb.Core;

namespace EvDb.IntegrationTests.EF.Events;

[EvDbDefineEventPayload("email-added")]
public readonly partial record struct PersonEmailAdded(int PersonId, string Email, string Category);

