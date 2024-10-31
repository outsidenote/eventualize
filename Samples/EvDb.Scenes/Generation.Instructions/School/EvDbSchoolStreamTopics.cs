// Ignore Spelling: TopicProducer Topic

using EvDb.Core;
using EvDb.Scenes;
using System.Collections.Immutable;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using VogenTableName;


namespace EvDb.UnitTests;



[EvDbAttachTopicTables<TopicTables>] 
[EvDbMessageTypes<AvgMessage>]
[EvDbMessageTypes<StudentPassedMessage>]
[EvDbMessageTypes<StudentFailedMessage>]
[EvDbTopic<SchoolStreamFactory>] // TODO: EvDbTopics
public partial class EvDbSchoolStreamTopics // TODO: MessageRouter
{
    protected override TopicTablesPreferences[] TopicToTables(EvDbSchoolStreamTopicOptions topic) => [TopicTablesPreferences.Topic];
        //topic switch
        //{
        //    EvDbSchoolStreamTopicOptions.Topic1 => [TopicTablesPreferences.Messaging],
        //    EvDbSchoolStreamTopicOptions.Topic3 => [
        //                                            TopicTablesPreferences.Commands,
        //                                            TopicTablesPreferences.Messaging],
        //    _ => []
        //};

    protected override void ProduceTopicMessages(EvDb.Scenes.StudentReceivedGradeEvent payload,
                                                 IEvDbEventMeta meta,
                                                 EvDbSchoolStreamViews views,
                                                 EvDbSchoolStreamTopicsContext topics)
    {
        Stats state = views.ALL;
        AvgMessage avg = new (state.Sum / (double)state.Count);
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
            topics.Add(pass, TopicsOfStudentPassedMessage.Topic1);
            topics.Add(pass, TopicsOfStudentPassedMessage.Topic3);
            topics.Add(pass);
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

