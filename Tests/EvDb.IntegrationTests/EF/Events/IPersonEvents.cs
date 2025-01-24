using EvDb.Core;

namespace EvDb.IntegrationTests.EF.Events;

[EvDbEventTypes<PersonAddressChanged>]
[EvDbEventTypes<PersonBirthdayChanged>]
[EvDbEventTypes<PersonEmailAdded>]
[EvDbEventTypes<PersonEmailCategoryUpdated>]
[EvDbEventTypes<PersonEmailRemoved>]
[EvDbEventTypes<PersonNameChanged>]
public partial interface IPersonEvents
{
}
