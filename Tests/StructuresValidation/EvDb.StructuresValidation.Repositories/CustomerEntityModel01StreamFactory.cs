using EvDb.Core;
using EvDb.StructuresValidation.Abstractions.Events;

namespace EvDb.StructuresValidation.Repositories;

[EvDbStreamFactory<ICustomerEntityModelEvents>("CLM", "CEM01")]
public partial class CustomerEntityModel01StreamFactory
{
}


[EvDbOutbox<CustomerEntityModel01StreamFactory>]
public partial class CustomerEntityModel01Outbox
{ 
}