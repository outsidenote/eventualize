using EvDb.Core;

namespace EvDb.Scenes;

public record StudentCourseApplicationDenied(int CourseId, int StudentId) : IEvDbEventPayload
{
    string IEvDbEventPayload.EventType { get; } = "student-course-application-denied";
}




