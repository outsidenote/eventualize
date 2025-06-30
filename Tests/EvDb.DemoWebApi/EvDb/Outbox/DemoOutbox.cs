// Ignore Spelling: OutboxProducer Channel

using EvDb.Core;

namespace EvDb.DemoWebApi.Outbox;

[EvDbAttachMessageType<CommentsMessage>]
[EvDbAttachMessageType<LivenessMessage>]
[EvDbAttachMessageType<RatingMessage>]
[EvDbOutbox<DemoStreamFactory>]
public partial class DemoOutbox
{
    protected override void ProduceOutboxMessages(CreatedEvent payload,
                                                  IEvDbEventMeta meta,
                                                  EvDbDemoStreamViews views,
                                                  DemoOutboxContext outbox)
    {
        outbox.Append(new LivenessMessage(meta.StreamCursor.StreamId, true, meta.CapturedAt));
    }

    protected override void ProduceOutboxMessages(ModifiedEvent payload,
                                                  IEvDbEventMeta meta,
                                                  EvDbDemoStreamViews views,
                                                  DemoOutboxContext outbox)
    {
        outbox.Append(new RatingMessage(meta.StreamCursor.StreamId, payload.Rate, meta.CapturedAt));
    }

    protected override void ProduceOutboxMessages(DeletedEvent payload,
                                                  IEvDbEventMeta meta,
                                                  EvDbDemoStreamViews views,
                                                  DemoOutboxContext outbox)
    {
        outbox.Append(new LivenessMessage(meta.StreamCursor.StreamId, false, meta.CapturedAt));
    }

    protected override void ProduceOutboxMessages(CommentedEvent payload,
                                                  IEvDbEventMeta meta,
                                                  EvDbDemoStreamViews views,
                                                  DemoOutboxContext outbox)
    {
        outbox.Append(new CommentsMessage(meta.StreamCursor.StreamId, views.Status.Comments.ToArray(), meta.CapturedAt));
    }
}

