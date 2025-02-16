﻿using EvDb.Core;
using EvDb.StructuresValidation.Abstractions.Events;

namespace EvDb.StructuresValidation.Repositories;

[EvDbStreamFactory<ICustomerEntityModelEvents, CustomerEntityModel01Outbox>("CLM", "CEM01")]
public partial class CustomerEntityModel01StreamFactory
{
}
