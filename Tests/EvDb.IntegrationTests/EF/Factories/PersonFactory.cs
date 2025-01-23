using EvDb.Core;
using EvDb.IntegrationTests.EF.Events;
using EvDb.IntegrationTests.EF.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvDb.IntegrationTests.EF.Factories;

[EvDbAttachView<PersonTypedView>("Typed")]
//[EvDbAttachView<PersonUntypedView>("Untyped")]
[EvDbStreamFactory<IPersonEvents>("community", "people")]
internal partial class PersonFactory
{
}
