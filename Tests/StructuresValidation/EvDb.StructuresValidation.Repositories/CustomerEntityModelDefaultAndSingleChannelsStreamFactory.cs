using EvDb.Core;
using EvDb.StructuresValidation.Abstractions.Events;

namespace EvDb.StructuresValidation.Repositories;

[EvDbStreamFactory<ICustomerEntityModelEvents, CustomerEntityModelDefaultAndSingleChannelsOutbox>("CLM:CEM03")]
public partial class CustomerEntityModelDefaultAndSingleChannelsStreamFactory
{
}
