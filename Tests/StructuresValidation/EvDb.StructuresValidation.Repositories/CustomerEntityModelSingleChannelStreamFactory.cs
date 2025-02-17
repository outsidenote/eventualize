using EvDb.Core;
using EvDb.StructuresValidation.Abstractions.Events;

namespace EvDb.StructuresValidation.Repositories;

[EvDbStreamFactory<ICustomerEntityModelEvents, CustomerEntityModelSingleChannelOutbox>("CLM", "CEM02")]
public partial class CustomerEntityModelSingleChannelStreamFactory
{
}
