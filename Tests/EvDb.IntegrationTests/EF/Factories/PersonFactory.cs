using EvDb.Core;
using EvDb.IntegrationTests.EF.Events;
using EvDb.IntegrationTests.EF.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvDb.IntegrationTests.EF.Factories;

[EvDbAttachView<PersonView>]
[EvDbStreamFactory<IPersonEvents>("community", "people")]
internal class PersonFactory
{
}
