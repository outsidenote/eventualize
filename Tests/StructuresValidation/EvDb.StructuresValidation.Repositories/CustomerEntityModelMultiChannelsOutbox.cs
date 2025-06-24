using EvDb.Core;
using EvDb.StructuresValidation.Abstractions;
using EvDb.StructuresValidation.Abstractions.Events;

namespace EvDb.StructuresValidation.Repositories;

[EvDbAttachMessageType<PersonChangedMultiChannelsMessage>]
[EvDbAttachMessageType<PersonChangedMessage>]
[EvDbOutbox<CustomerEntityModelMultiChannelsStreamFactory>]
public partial class CustomerEntityModelMultiChannelsOutbox
{
    protected override void ProduceOutboxMessages(EmailValidatedEvent payload, IEvDbEventMeta meta, EvDbCustomerEntityModelMultiChannelsStreamViews views, CustomerEntityModelMultiChannelsOutboxContext outbox)
    {
        var personChanged = new PersonChangedMultiChannelsMessage
        {
            Id = meta.StreamCursor.StreamId,
            Email = payload.Email,
            EmailIsValid = payload.IsValid
        };
        outbox.Append(personChanged, PersonChangedMultiChannelsMessageChannels.Channel1);
        outbox.Append(personChanged, PersonChangedMultiChannelsMessageChannels.Channel2);
    }
}