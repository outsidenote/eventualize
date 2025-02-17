using EvDb.Core;
using EvDb.StructuresValidation.Abstractions.Events;

namespace EvDb.StructuresValidation.Repositories;

[EvDbStreamFactory<ICustomerEntityModelEvents, CustomerEntityModelMultiChannelsOutbox>("CLM", "CEM04")]
public partial class CustomerEntityModelMultiChannelsStreamFactory
{
}
