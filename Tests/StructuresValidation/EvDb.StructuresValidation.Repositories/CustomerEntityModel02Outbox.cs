using EvDb.Core;
using EvDb.StructuresValidation.Abstractions;
using EvDb.StructuresValidation.Abstractions.Events;

namespace EvDb.StructuresValidation.Repositories;

[EvDbMessageTypes<PersonChanged02Message>]
[EvDbMessageTypes<PersonChangedMessage>]
[EvDbOutbox<CustomerEntityModel02StreamFactory>]
public partial class CustomerEntityModel02Outbox
{
    protected override void ProduceOutboxMessages(EmailValidatedEvent payload,
                                                  IEvDbEventMeta meta,
                                                  EvDbCustomerEntityModel02StreamViews views,
                                                  CustomerEntityModel02OutboxContext outbox)
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