using EvDb.Core;

namespace EvDb.Scenes;

[EvDbDefineEventPayload("student-course-application-denied")]
public readonly partial record struct StudentCourseApplicationDeniedEvent(int CourseId, int StudentId);




