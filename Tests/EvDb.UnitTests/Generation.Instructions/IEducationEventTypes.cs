using EvDb.Core;
using EvDb.Scenes;


namespace EvDb.UnitTests;


[EvDbEventAdder<CourseCreatedEvent>]
[EvDbEventAdder<ScheduleTestEvent>]
[EvDbEventAdder<StudentAppliedToCourseEvent>]
[EvDbEventAdder<StudentCourseApplicationDeniedEvent>]
[EvDbEventAdder<StudentEnlistedEvent>]
[EvDbEventAdder<StudentQuitCourseEvent>]
[EvDbEventAdder<StudentReceivedGradeEvent>]
[EvDbEventAdder<StudentRegisteredToCourseEvent>]
[EvDbEventAdder<StudentTestSubmittedEvent>]
public partial interface IEducationEventTypes
{
}
