using EvDb.Core;

namespace EvDb.Scenes;

public record StudentReceivedGradeEvent(int TestId, int StudentId, double Grade, string? Comments = null) : IEvDbEventPayload
{
    string IEvDbEventPayload.EventType { get; } = "student-received-grade";
}



