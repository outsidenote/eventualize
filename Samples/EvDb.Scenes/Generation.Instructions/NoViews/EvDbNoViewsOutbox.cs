// Ignore Spelling: OutboxProducer Channel

using EvDb.Core;
using EvDb.Scenes;

namespace EvDb.UnitTests;

[EvDbAttachMessageType<AvgMessage>]
[EvDbOutbox<NoViewsFactory>]
public partial class EvDbNoViewsOutbox
{
    protected override void ProduceOutboxMessages(EvDb.Scenes.StudentReceivedGradeEvent payload,
                                                 IEvDbEventMeta meta,
                                                 EvDbNoViewsViews views,
                                                 EvDbNoViewsOutboxContext outbox)
    {
        AvgMessage avg = new(payload.Grade);
        outbox.Add(avg);
    }
}
