// Ignore Spelling: OutboxProducer Channel

using EvDb.Core;
using EvDb.Scenes;

namespace EvDb.UnitTests;

[EvDbAttachMessageType<AvgMessage>]
[EvDbAttachMessageType<StudentPassedMessage>]
[EvDbAttachMessageType<StudentFailedMessage>]
[EvDbOutbox<SchoolStreamFactory, OutboxShards>]
[EvDbUseOutboxSerialization<AvroSerializer, PrefixSerializer>(EvDbOutboxSerializationMode.Strict)]
public partial class EvDbSchoolOutbox
{
    protected override Shards[] ChannelToShards(Channels outbox) =>
        outbox switch
        {
            Channels.Channel1 => [Shards.Commands],
            Channels.Channel2 => [Shards.Messaging],
            Channels.Channel3 => [Shards.MessagingVip, Shards.Messaging],
            _ => []
        };

    protected override void ProduceOutboxMessages(EvDb.Scenes.StudentReceivedGradeEvent payload,
                                                 IEvDbEventMeta meta,
                                                 EvDbSchoolStreamViews views,
                                                 EvDbSchoolOutboxContext outbox)
    {
        Stats state = views.ALL;
        AvgMessage avg = new(state.Sum / (double)state.Count);
        outbox.Append(avg);
        var studentName = views.StudentStats.Students
            .First(m => m.StudentId == payload.StudentId)
            .StudentName;
        if (payload.Grade >= 60)
        {
            var pass = new StudentPassedMessage(payload.StudentId,
                                             studentName,
                                             meta.CapturedAt,
                                             payload.Grade);
            outbox.Append(pass, StudentPassedMessage.Channels.Channel2);
            outbox.Append(pass, StudentPassedMessage.Channels.Channel3);
        }
        else
        {
            var fail = new StudentFailedMessage(payload.StudentId,
                                             studentName,
                                             meta.CapturedAt,
                                             payload.Grade);
            outbox.Append(fail);
        }
    }
}
