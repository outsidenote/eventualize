using EvDb.Core;

namespace EvDb.IntegrationTests.EF.Events;

[EvDbDefineEventPayload("name")]
public readonly partial record struct PersonNameChanged(int Id, string Name);

