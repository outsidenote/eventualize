using EvDb.Core;

namespace EvDb.Scenes;

public record StudentCourseApplicationDeniedEvent(int CourseId, int StudentId) : IEvDbEventPayload
{
    string IEvDbEventPayload.EventType { get; } = "student-course-application-denied";
}




