// Ignore Spelling: OutboxProducer Channel

using EvDb.Core;
using EvDb.Scenes;

namespace EvDb.UnitTests;

[EvDbAttachMessageType<StudentPassedMessage>]
[EvDbAttachMessageType<AvgMessage>]
[EvDbOutbox<NoViewsFactory>("messages")]
public partial class EvDbNoViewsOutbox
{
    protected override void ProduceOutboxMessages(EvDb.Scenes.StudentReceivedGradeEvent payload,
                                                 IEvDbEventMeta meta,
                                                 EvDbNoViewsViews views,
                                                 EvDbNoViewsOutboxContext outbox)
    {
        switch (meta.StreamCursor.Offset % 4)
        {
            case 0:
                AvgMessage m0 = new(payload.Grade);
                outbox.Append(m0);
                break;
            case 1:
                StudentPassedMessage m1 = new(payload.StudentId, "John", DateTimeOffset.UtcNow, payload.Grade);
                outbox.Append(m1, StudentPassedMessageChannels.Channel1);
                break;
            case 2:
                StudentPassedMessage m2 = new(payload.StudentId, "John", DateTimeOffset.UtcNow, payload.Grade);
                outbox.Append(m2, StudentPassedMessageChannels.Channel2);
                break;
            case 3:
                StudentPassedMessage m3 = new(payload.StudentId, "John", DateTimeOffset.UtcNow, payload.Grade);
                outbox.Append(m3, StudentPassedMessageChannels.Channel3);
                break;
        }

    }
}
