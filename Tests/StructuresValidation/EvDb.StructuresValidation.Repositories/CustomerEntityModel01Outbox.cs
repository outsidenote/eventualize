using EvDb.Core;
using EvDb.StructuresValidation.Abstractions.Events;

namespace EvDb.StructuresValidation.Repositories;

[EvDbMessageTypes<PersonChangedMessage>]
[EvDbOutbox<CustomerEntityModel01StreamFactory>]
public partial class CustomerEntityModel01Outbox
{
    protected override void ProduceOutboxMessages(EmailValidatedEvent payload,
                                                  IEvDbEventMeta meta,
                                                  EvDbCustomerEntityModel01StreamViews views,
                                                  CustomerEntityModel01OutboxContext outbox)
    {
        var personChanged = new PersonChangedMessage
        {
            Id = meta.StreamCursor.StreamId,
            Email = payload.Email,
            EmailIsValid = payload.IsValid
        };
        outbox.Add(personChanged);
    }
}