using EvDb.Core;
using EvDb.Scenes;
using EvDb.UnitTests;


namespace EvDb.UnitTests;

[EvDbEventType<CourseCreated>("course-created")]
[EvDbEventType<ScheduleTest>("schedule-test")]
[EvDbEventType<StudentAppliedToCourse>("student-applied-to-course")]
[EvDbEventType<StudentCourseApplicationDenied>("student-course-application-denied")]
[EvDbEventType<StudentEnlisted>("student-enlisted")]
[EvDbEventType<StudentQuitCourse>("student-quit-course")]
[EvDbEventType<StudentReceivedGrade>("student-received-grade")]
[EvDbEventType<StudentRegisteredToCourse>("student-registered-to-course")]
[EvDbEventType<StudentTestSubmitted>("StudentTestSubmitted")]
public partial interface IEducationEventTypes
{
}

//[EvDbEventType<CourseCreated>("course-created")]
//[EvDbEventType<ScheduleTest>("schedule-test")]
//[EvDbEventType<StudentAppliedToCourse>("student-applied-to-course")]
//[EvDbEventType<StudentCourseApplicationDenied>("student-course-application-denied")]
//[EvDbEventType<StudentEnlisted>("student-enlisted")]
//[EvDbEventType<StudentQuitCourse>("student-quit-course")]
//[EvDbEventType<StudentReceivedGrade>("student-received-grade")]
//[EvDbEventType<StudentRegisteredToCourse>("student-registered-to-course")]
//[EvDbEventType<StudentTestSubmitted>("StudentTestSubmitted")]
//public partial record EvDbPartiotion<TEventtypes>:EvDbPartitionAddress
//    where TEventtypes: IEvDbEventTypes;