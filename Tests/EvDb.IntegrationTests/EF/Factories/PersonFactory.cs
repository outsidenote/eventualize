using EvDb.Core;
using EvDb.IntegrationTests.EF.Events;
using EvDb.IntegrationTests.EF.Views;

namespace EvDb.IntegrationTests.EF.Factories;

[EvDbAttachView<PersonTypedView>("Typed")]
[EvDbAttachView<PersonTyped2View>("Typed2")]
[EvDbAttachView<PersonUntypedView>("Untyped")]
[EvDbStreamFactory<IPersonEvents>("community:people")]
internal partial class PersonFactory
{
}
