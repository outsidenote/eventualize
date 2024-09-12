using EvDb.Core;
using EvDb.Scenes;


namespace EvDb.UnitTests;

//[EvDbEventTypes<CourseCreatedPublicEvent>]
//[EvDbEventTypes<StudentQuitCoursePublicEvent>]
//public partial interface IEvDbSchoolStreamPublication
//{
//}

[EvDbPublicationTypes<CourseCreatedPublicEvent>]
[EvDbPublicationTypes<StudentQuitCoursePublicEvent>]
public partial class EvDbSchoolStreamPublicationFold
{

    protected override void Publication(
        StudentEnlistedEvent payload,
        EvDbSchoolStreamViews views,
        IEvDbEventMeta meta,
        SchoolStreamPublicationProducer producer)
    {
        var p = new CourseCreatedPublicEvent(payload.Student.Id, payload.Student.Name, 10);
        producer.Publish(p); // --> call Publish()
    }

    protected override void Publication(
        ScheduleTestEvent payload,
        EvDbSchoolStreamViews views,
        IEvDbEventMeta meta,
        SchoolStreamPublicationProducer producer)
    {
        //var p = new StudentQuitCoursePublicEvent(payload.)
        //producer.Publish(p); // --> call Publish()
    }
}

