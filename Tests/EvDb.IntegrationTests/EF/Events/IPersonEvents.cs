using EvDb.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
