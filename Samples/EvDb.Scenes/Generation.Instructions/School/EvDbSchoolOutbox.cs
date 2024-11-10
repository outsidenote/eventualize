// Ignore Spelling: OutboxProducer Channel

using EvDb.Core;
using EvDb.Scenes;
using Microsoft.Extensions.Logging;

namespace EvDb.UnitTests;

[EvDbMessageTypes<AvgMessage>]
[EvDbMessageTypes<StudentPassedMessage>]
[EvDbMessageTypes<StudentFailedMessage>]
[EvDbOutbox<SchoolStreamFactory, OutboxShards>]
[EvDbUseOutboxSerialization<AvroSerializer, PrefixSerializer>(EvDbOutboxSerializationMode.Strict)] 
public partial class EvDbSchoolOutbox // TODO: MessageRouter / Outbox
{
    protected override OutboxShardsPreferences[] ChannelToShards(EvDbSchoolOutboxChannels outbox) =>
        outbox switch
        {
            // TODO: change the base name of the enum to use EvDbSchoolOutbox
            EvDbSchoolOutboxChannels.Channel1 => [OutboxShardsPreferences.Commands],
            EvDbSchoolOutboxChannels.Channel2 => [
                                                    OutboxShardsPreferences.Messaging],
            EvDbSchoolOutboxChannels.Channel3 => [
                                                    OutboxShardsPreferences.MessagingVip,
                                                    OutboxShardsPreferences.Messaging],
            _ => []
        };

    protected override void ProduceOutboxMessages(EvDb.Scenes.StudentReceivedGradeEvent payload,
                                                 IEvDbEventMeta meta,
                                                 EvDbSchoolStreamViews views,
                                                 EvDbSchoolOutboxContext topics)
    {
        Stats state = views.ALL;
        AvgMessage avg = new(state.Sum / (double)state.Count);
        topics.Add(avg);
        var studentName = views.StudentStats.Students
            .First(m => m.StudentId == payload.StudentId)
            .StudentName;
        if (payload.Grade >= 60)
        {
            var pass = new StudentPassedMessage(payload.StudentId,
                                             studentName,
                                             meta.CapturedAt,
                                             payload.Grade);
            topics.Add(pass, OutboxOfStudentPassedMessage.Channel2);
            topics.Add(pass, OutboxOfStudentPassedMessage.Channel3);
        }
        else
        {
            var fail = new StudentFailedMessage(payload.StudentId,
                                             studentName,
                                             meta.CapturedAt,
                                             payload.Grade);
            topics.Add(fail);
        }
    }
}

