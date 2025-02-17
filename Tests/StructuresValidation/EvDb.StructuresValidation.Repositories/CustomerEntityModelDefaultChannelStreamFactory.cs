using EvDb.Core;
using EvDb.StructuresValidation.Abstractions.Events;

namespace EvDb.StructuresValidation.Repositories;

[EvDbStreamFactory<ICustomerEntityModelEvents, CustomerEntityModelDefaultChannelOutbox>("CLM", "CEM03")]
public partial class CustomerEntityModelDefaultChannelStreamFactory
{
}
