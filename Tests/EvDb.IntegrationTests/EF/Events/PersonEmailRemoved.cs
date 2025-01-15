using EvDb.Core;

namespace EvDb.IntegrationTests.EF.Events;

[EvDbDefineEventPayload("email-removed")]
public readonly partial record struct PersonEmailRemoved(int PersonId, string Email, string Category);

