using EvDb.Core;

namespace EvDb.IntegrationTests.EF.Events;

[EvDbAttachEventType<PersonAddressChanged>]
[EvDbAttachEventType<PersonBirthdayChanged>]
[EvDbAttachEventType<PersonEmailAdded>]
[EvDbAttachEventType<PersonEmailCategoryUpdated>]
[EvDbAttachEventType<PersonEmailRemoved>]
[EvDbAttachEventType<PersonNameChanged>]
public partial interface IPersonEvents
{
}
