// Ignore Spelling: TopicProducer Topic

using EvDb.Core;
using EvDb.Scenes;


namespace EvDb.UnitTests;


[EvDbMessageTypes<AvgTopic>]
[EvDbMessageTypes<StudentPassTopic>]
[EvDbMessageTypes<StudentFailTopic>]
[EvDbTopic<SchoolStreamFactory>]
public partial class EvDbSchoolStreamTopics
{
    protected override void ProduceTopicMessages(
        StudentReceivedGradeEvent payload,
        EvDbSchoolStreamViews views,
        IEvDbEventMeta meta,
        EvDbSchoolStreamTopicsContext topics)
    {
        var state = views.ALL;
        var avg = new AvgTopic(state.Sum / (double)state.Count);
        topics.Add(avg);
        var studentName = views.StudentStats.Students
            .First(m => m.StudentId == payload.StudentId)
            .StudentName;
        if (payload.Grade >= 60)
        {
            var pass = new StudentPassTopic(payload.StudentId,
                                             studentName,
                                             meta.CapturedAt,
                                             payload.Grade);
            topics.Topic1.Add(pass);
            //topics.Add(pass, StudentPassTopicTopics.Topic1);
            //topics.Topic1.Add(pass);
            //topics.Topic2.Add(pass);
        }
        else
        { 
            var fail = new StudentFailTopic(payload.StudentId,
                                             studentName,
                                             meta.CapturedAt,
                                             payload.Grade);
            topics.Topic2.Add(fail);
        }
    }    
}

