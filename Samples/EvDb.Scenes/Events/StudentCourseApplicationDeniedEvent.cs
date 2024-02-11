using EvDb.Core;

namespace EvDb.Scenes;

[EvDbEventPayload("student-course-application-denied")]
public readonly partial record struct StudentCourseApplicationDeniedEvent(int CourseId, int StudentId);




