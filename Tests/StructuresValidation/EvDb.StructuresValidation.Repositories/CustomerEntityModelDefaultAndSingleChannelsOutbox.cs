using EvDb.Core;
using EvDb.StructuresValidation.Abstractions;
using EvDb.StructuresValidation.Abstractions.Events;

namespace EvDb.StructuresValidation.Repositories;

[EvDbMessageTypes<PersonChangedDefaultAndSingleChannelsMessage>]
[EvDbMessageTypes<PersonChangedMessage>]
[EvDbOutbox<CustomerEntityModelDefaultAndSingleChannelsStreamFactory>]
public partial class CustomerEntityModelDefaultAndSingleChannelsOutbox
{
    protected override void ProduceOutboxMessages(EmailValidatedEvent payload, IEvDbEventMeta meta, EvDbCustomerEntityModelDefaultAndSingleChannelsStreamViews views, CustomerEntityModelDefaultAndSingleChannelsOutboxContext outbox)
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