// Ignore Spelling: OutboxHandler Outbox

using EvDb.Core;
using EvDb.Scenes;


namespace EvDb.UnitTests;


[EvDbOutboxTypes<CourseCreatedOutboxEvent>]
[EvDbOutboxTypes<StudentQuitCourseOutboxEvent>]
[EvDbOutbox<SchoolStreamFactory>]
public partial class EvDbSchoolStreamOutbox
{
    protected override void OutboxHandler(
        StudentEnlistedEvent payload,
        EvDbSchoolStreamViews views,
        IEvDbEventMeta meta,
        EvDbSchoolStreamOutboxContext outbox)
    {
        var p = new CourseCreatedOutboxEvent(payload.Student.Id, payload.Student.Name, 10);
        outbox.Add(p);
    }

    protected override void OutboxHandler(
        ScheduleTestEvent payload,
        EvDbSchoolStreamViews views,
        IEvDbEventMeta meta,
        EvDbSchoolStreamOutboxContext outbox)
    {
        //var p = new StudentQuitCourseOutboxEvent(payload.)
        //outbox.AddToOutbox(p); // --> call AddToOutbox()
    }
}

