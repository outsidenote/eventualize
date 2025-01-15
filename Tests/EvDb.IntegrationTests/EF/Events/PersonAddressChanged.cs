using EvDb.Core;

namespace EvDb.IntegrationTests.EF.Events;

[EvDbDefineEventPayload("address")]
public readonly partial record struct PersonAddressChanged(int Id, Address Address);

