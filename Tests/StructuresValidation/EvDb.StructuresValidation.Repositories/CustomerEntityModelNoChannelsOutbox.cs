using EvDb.Core;
using EvDb.StructuresValidation.Abstractions;
using EvDb.StructuresValidation.Abstractions.Events;

namespace EvDb.StructuresValidation.Repositories;

[EvDbAttachMessageType<PersonChangedMessage>]
[EvDbOutbox<CustomerEntityModelNoChannelsStreamFactory>]
public partial class CustomerEntityModelNoChannelsOutbox
{
    protected override void ProduceOutboxMessages(EmailValidatedEvent payload, IEvDbEventMeta meta, EvDbCustomerEntityModelNoChannelsStreamViews views, CustomerEntityModelNoChannelsOutboxContext outbox)
    {
        var personChanged = new PersonChangedMessage
        {
            Id = meta.StreamCursor.StreamId,
            Email = payload.Email,
            EmailIsValid = payload.IsValid
        };
        outbox.Append(personChanged);
    }
}