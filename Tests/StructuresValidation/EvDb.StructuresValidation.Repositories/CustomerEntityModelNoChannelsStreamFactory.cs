using EvDb.Core;
using EvDb.StructuresValidation.Abstractions.Events;

namespace EvDb.StructuresValidation.Repositories;

[EvDbStreamFactory<ICustomerEntityModelEvents, CustomerEntityModelNoChannelsOutbox>("CLM", "CEM01")]
public partial class CustomerEntityModelNoChannelsStreamFactory
{
}
