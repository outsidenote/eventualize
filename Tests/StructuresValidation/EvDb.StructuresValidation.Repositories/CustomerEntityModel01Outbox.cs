using EvDb.Core;
using EvDb.StructuresValidation.Abstractions.Events;

namespace EvDb.StructuresValidation.Repositories;

[EvDbMessageTypes<PersonChangedMessage>]
[EvDbOutbox<CustomerEntityModel01StreamFactory>]
public partial class CustomerEntityModel01Outbox
{ 
}