using EvDb.Core;
using EvDb.Scenes;


namespace EvDb.UnitTests;

[EvDbAttachEventType<CourseCreatedEvent>]
[EvDbAttachEventType<ScheduleTestEvent>]
[EvDbAttachEventType<StudentAppliedToCourseEvent>]
[EvDbAttachEventType<StudentCourseApplicationDeniedEvent>]
[EvDbAttachEventType<StudentEnlistedEvent>]
[EvDbAttachEventType<StudentQuitCourseEvent>]
[EvDbAttachEventType<StudentReceivedGradeEvent>]
[EvDbAttachEventType<StudentRegisteredToCourseEvent>]
[EvDbAttachEventType<StudentTestSubmittedEvent>]
public partial interface IEvDbSchoolStreamAdders
{
}

[EvDbAttachEventType<CourseCreatedEvent>]
[EvDbAttachEventType<ScheduleTestEvent>]
[EvDbAttachEventType<StudentAppliedToCourseEvent>]
[EvDbAttachEventType<StudentCourseApplicationDeniedEvent>]
[EvDbAttachEventType<StudentEnlistedEvent>]
[EvDbAttachEventType<StudentQuitCourseEvent>]
[EvDbAttachEventType<StudentReceivedGradeEvent>]
[EvDbAttachEventType<StudentRegisteredToCourseEvent>]
[EvDbAttachEventType<StudentTestSubmittedEvent>]
public partial interface IEvDbSchoolStreamAdders1
{
}

