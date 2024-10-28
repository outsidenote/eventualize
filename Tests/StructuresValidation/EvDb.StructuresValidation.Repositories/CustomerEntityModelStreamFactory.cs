using EvDb.Core;
using EvDb.StructuresValidation.Abstractions.Events;

namespace EvDb.StructuresValidation.Repositories;

[EvDbStreamFactory<ICustomerEntityModelEvents>("CLM", "CEM")]
public partial class CustomerEntityModelStreamFactory
{
}