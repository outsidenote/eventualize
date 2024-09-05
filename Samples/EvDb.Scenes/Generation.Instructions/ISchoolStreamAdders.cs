using EvDb.Core;
using EvDb.Scenes;


namespace EvDb.UnitTests;


//[EvDbEventPublication<CourseCreatedPublicEvent>]
//[EvDbEventPublication<StudentQuitCoursePublicEvent>]
[EvDbEventTypes<CourseCreatedPublicEvent>]
[EvDbEventTypes<StudentQuitCoursePublicEvent>]
public partial interface IEvDbSchoolStreamPublication
{
}

[EvDbEventTypes<CourseCreatedEvent>]
[EvDbEventTypes<ScheduleTestEvent>]
[EvDbEventTypes<StudentAppliedToCourseEvent>]
[EvDbEventTypes<StudentCourseApplicationDeniedEvent>]
[EvDbEventTypes<StudentEnlistedEvent>]
[EvDbEventTypes<StudentQuitCourseEvent>]
[EvDbEventTypes<StudentReceivedGradeEvent>]
[EvDbEventTypes<StudentRegisteredToCourseEvent>]
[EvDbEventTypes<StudentTestSubmittedEvent>]
public partial interface IEvDbSchoolStreamAdders
{
}

[EvDbEventTypes<CourseCreatedEvent>]
[EvDbEventTypes<ScheduleTestEvent>]
[EvDbEventTypes<StudentAppliedToCourseEvent>]
[EvDbEventTypes<StudentCourseApplicationDeniedEvent>]
[EvDbEventTypes<StudentEnlistedEvent>]
[EvDbEventTypes<StudentQuitCourseEvent>]
[EvDbEventTypes<StudentReceivedGradeEvent>]
[EvDbEventTypes<StudentRegisteredToCourseEvent>]
[EvDbEventTypes<StudentTestSubmittedEvent>]
public partial interface IEvDbSchoolStreamAdders1
{
}

