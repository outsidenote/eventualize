using EvDb.Core;
using EvDb.StructuresValidation.Abstractions;
using EvDb.StructuresValidation.Abstractions.Events;

namespace EvDb.StructuresValidation.Repositories;

[EvDbAttachMessageType<PersonChangedDefaultChannelMessage>]
[EvDbAttachMessageType<PersonChangedMessage>]
[EvDbOutbox<CustomerEntityModelDefaultChannelStreamFactory>]
public partial class CustomerEntityModelDefaultChannelOutbox
{
    protected override void ProduceOutboxMessages(EmailValidatedEvent payload, IEvDbEventMeta meta, EvDbCustomerEntityModelDefaultChannelStreamViews views, CustomerEntityModelDefaultChannelOutboxContext outbox)
    {
        var personChanged = new PersonChangedDefaultChannelMessage
        {
            Id = meta.StreamCursor.StreamId,
            Email = payload.Email,
            EmailIsValid = payload.IsValid
        };
        outbox.Add(personChanged);
    }
}