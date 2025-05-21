using EvDb.Core;
using EvDb.StructuresValidation.Abstractions.Events;

namespace EvDb.StructuresValidation.Repositories;

[EvDbStreamFactory<ICustomerEntityModelEvents, CustomerEntityModelNoChannelsOutbox>("CLM_CEM01")]
public partial class CustomerEntityModelNoChannelsStreamFactory
{
}
