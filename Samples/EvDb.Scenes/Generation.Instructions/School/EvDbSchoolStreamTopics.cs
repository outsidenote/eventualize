// Ignore Spelling: TopicProducer Topic

using EvDb.Core;
using EvDb.Scenes;

namespace EvDb.UnitTests;

[EvDbMessageTypes<AvgMessage>]
[EvDbMessageTypes<StudentPassedMessage>]
[EvDbMessageTypes<StudentFailedMessage>]
[EvDbAttachOutboxTables<TopicTables>] // TODO: merge it into [EvDbOutbox]
[EvDbOutbox<SchoolStreamFactory>]
public partial class EvDbSchoolOutbox // TODO: MessageRouter / Outbox
{
    protected override TopicTablesPreferences[] TopicToTables(EvDbSchoolStreamOutboxOptions topic) =>
        topic switch
        {
            EvDbSchoolStreamOutboxOptions.Topic1 => [TopicTablesPreferences.Commands],
            EvDbSchoolStreamOutboxOptions.Topic2 => [
                                                    TopicTablesPreferences.Messaging],
            EvDbSchoolStreamOutboxOptions.Topic3 => [
                                                    TopicTablesPreferences.MessagingVip,
                                                    TopicTablesPreferences.Messaging],
            _ => []
        };

    protected override void ProduceTopicMessages(EvDb.Scenes.StudentReceivedGradeEvent payload,
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
            topics.Add(pass, OutboxOfStudentPassedMessage.Topic2);
            topics.Add(pass, OutboxOfStudentPassedMessage.Topic3);
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

