using EvDb.Core;

namespace EvDb.Scenes;

[EvDbEventPayload("student-course-application-denied")]
public partial record StudentCourseApplicationDeniedEvent(int CourseId, int StudentId);




