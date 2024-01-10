using EvDb.Core;
using EvDb.Scenes;
using EvDb.UnitTests;
using System.Text.Json.Serialization;


namespace EvDb.UnitTests;


[EvDbEventType<CourseCreatedEvent>]
[EvDbEventType<ScheduleTestEvent>]
[EvDbEventType<StudentAppliedToCourseEvent>]
[EvDbEventType<StudentCourseApplicationDeniedEvent>]
[EvDbEventType<StudentEnlistedEvent>]
[EvDbEventType<StudentQuitCourseEvent>]
[EvDbEventType<StudentReceivedGradeEvent>]
[EvDbEventType<StudentRegisteredToCourseEvent>]
[EvDbEventType<StudentTestSubmittedEvent>]
public partial interface IEducationEventTypes
{
}
