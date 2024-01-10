using EvDb.Core;

namespace EvDb.Scenes;

public record StudentRegisteredToCourseEvent(int CourseId, StudentEntity Student) : IEvDbEventPayload
{
    string IEvDbEventPayload.EventType { get; } = "student-registered-to-course";
}



