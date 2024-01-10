using EvDb.Core;

namespace EvDb.Scenes;

public record StudentQuitCourseEvent(int CourseId, int StudentId) : IEvDbEventPayload
{
    string IEvDbEventPayload.EventType { get; } = "student-quit-course";
}





