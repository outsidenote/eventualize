using EvDb.Core;
using EvDb.StructuresValidation.Abstractions.Events;

namespace EvDb.StructuresValidation.Repositories;

[EvDbStreamFactory<ICustomerEntityModelEvents, CustomerEntityModel02Outbox>("CLM", "CEM02")]
public partial class CustomerEntityModel02StreamFactory
{
}
