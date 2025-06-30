using EvDb.Core;
using EvDb.StructuresValidation.Abstractions;
using EvDb.StructuresValidation.Abstractions.Events;

namespace EvDb.StructuresValidation.Repositories;

[EvDbAttachMessageType<PersonChangedDefaultAndSingleChannelsMessage>]
[EvDbAttachMessageType<PersonChangedMessage>]
[EvDbOutbox<CustomerEntityModelDefaultAndSingleChannelsStreamFactory>]
public partial class CustomerEntityModelDefaultAndSingleChannelsOutbox
{
    protected override void ProduceOutboxMessages(EmailValidatedEvent payload, IEvDbEventMeta meta, EvDbCustomerEntityModelDefaultAndSingleChannelsStreamViews views, CustomerEntityModelDefaultAndSingleChannelsOutboxContext outbox)
    {
        var personChanged = new PersonChangedDefaultAndSingleChannelsMessage
        {
            Id = meta.StreamCursor.StreamId,
            Email = payload.Email,
            EmailIsValid = payload.IsValid
        };
        outbox.Append(personChanged, PersonChangedDefaultAndSingleChannelsMessageChannels.Channel1);
    }
}