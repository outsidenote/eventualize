using EvDb.Core;

namespace EvDb.IntegrationTests.EF.Events;

[EvDbDefineEventPayload("birthy")]
public readonly partial record struct PersonBirthdayChanged(int Id, DateOnly Date);

