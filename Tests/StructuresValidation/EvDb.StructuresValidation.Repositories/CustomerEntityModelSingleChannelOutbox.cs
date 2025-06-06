﻿using EvDb.Core;
using EvDb.StructuresValidation.Abstractions;
using EvDb.StructuresValidation.Abstractions.Events;

namespace EvDb.StructuresValidation.Repositories;

[EvDbAttachMessageType<PersonChangedSingleChannelMessage>]
[EvDbAttachMessageType<PersonChangedMessage>]
[EvDbOutbox<CustomerEntityModelSingleChannelStreamFactory>]
public partial class CustomerEntityModelSingleChannelOutbox
{
    protected override void ProduceOutboxMessages(EmailValidatedEvent payload, IEvDbEventMeta meta, EvDbCustomerEntityModelSingleChannelStreamViews views, CustomerEntityModelSingleChannelOutboxContext outbox)
    {
        var personChanged = new PersonChangedSingleChannelMessage
        {
            Id = meta.StreamCursor.StreamId,
            Email = payload.Email,
            EmailIsValid = payload.IsValid
        };
        outbox.Append(personChanged);
    }
}