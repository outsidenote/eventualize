// Ignore Spelling: OutboxProducer Channel

using EvDb.Core;
using EvDb.Scenes;

namespace EvDb.UnitTests;

[EvDbAttachMessageType<AvgMessage>]
[EvDbAttachMessageType<StudentPassedMessage>]
[EvDbAttachMessageType<StudentFailedMessage>]
[EvDbOutbox<DuplicateFactory>]
public partial class EvDbDuplicateOutbox
{
    protected override void ProduceOutboxMessages(StudentReceivedGradeEvent payload, IEvDbEventMeta meta, EvDbDuplicateViews views, EvDbDuplicateOutboxContext outbox)
    {
        Stats state = views.Duplicate;
        AvgMessage avg = new(state.Sum / (double)state.Count);
        outbox.Add(avg);
    }
}
