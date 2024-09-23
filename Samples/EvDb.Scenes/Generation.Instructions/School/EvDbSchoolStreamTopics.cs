// Ignore Spelling: TopicProducer Topic

using EvDb.Core;
using EvDb.Scenes;


namespace EvDb.UnitTests;


[EvDbMessageTypes<AvgMessage>]
[EvDbMessageTypes<StudentPassedMessage>]
[EvDbMessageTypes<StudentFailedMessage>]
[EvDbTopic<SchoolStreamFactory>]
public partial class EvDbSchoolStreamTopics
{
    protected override void ProduceTopicMessages(
        StudentReceivedGradeEvent payload,
        IEvDbEventMeta meta,
        EvDbSchoolStreamViews views,
        EvDbSchoolStreamTopicsContext topics)
    {
        var state = views.ALL;
        var avg = new AvgMessage(state.Sum / (double)state.Count);
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
            //topics.Topic1.Add(pass);
            //topics.Topic2.Add(pass);
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

