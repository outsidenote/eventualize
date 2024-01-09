using EvDb.Core;
using EvDb.Scenes;
using EvDb.UnitTests;
using System.Text.Json.Serialization;


namespace EvDb.UnitTests;


[EvDbEventType<CourseCreated>]
[EvDbEventType<ScheduleTest>]
[EvDbEventType<StudentAppliedToCourse>]
[EvDbEventType<StudentCourseApplicationDenied>]
[EvDbEventType<StudentEnlisted>]
[EvDbEventType<StudentQuitCourse>]
[EvDbEventType<StudentReceivedGrade>]
[EvDbEventType<StudentRegisteredToCourse>]
[EvDbEventType<StudentTestSubmitted>]
public partial interface IEducationEventTypes
{
}
