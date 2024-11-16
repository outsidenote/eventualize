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
    protected override OutboxShardsPreferences[] ChannelToShards(Channels outbox) =>
        outbox switch
        {
            // TODO: [bnaya 2024-11-16] OutboxShardsPreferences -> EvDbSchoolOutbox.Shards -> Shards
            Channels.Channel1 => [OutboxShardsPreferences.Commands],
            Channels.Channel2 => [
                                                    OutboxShardsPreferences.Messaging],
            Channels.Channel3 => [
                                                    OutboxShardsPreferences.MessagingVip,
                                                    OutboxShardsPreferences.Messaging],
            _ => []
        };

    protected override void ProduceOutboxMessages(EvDb.Scenes.StudentReceivedGradeEvent payload,
                                                 IEvDbEventMeta meta,
                                                 EvDbSchoolStreamViews views,
                                                 EvDbSchoolOutboxContext outbox)
    {
        Stats state = views.ALL;
        AvgMessage avg = new(state.Sum / (double)state.Count);
        outbox.Add(avg);
        var studentName = views.StudentStats.Students
            .First(m => m.StudentId == payload.StudentId)
            .StudentName;
        if (payload.Grade >= 60)
        {
            var pass = new StudentPassedMessage(payload.StudentId,
                                             studentName,
                                             meta.CapturedAt,
                                             payload.Grade);
            outbox.Add(pass, StudentPassedMessage.Channels.Channel2);
            outbox.Add(pass, StudentPassedMessage.Channels.Channel3);
        }
        else
        {
            var fail = new StudentFailedMessage(payload.StudentId,
                                             studentName,
                                             meta.CapturedAt,
                                             payload.Grade);
            outbox.Add(fail);
        }
    }
}

