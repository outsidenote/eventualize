using EvDb.Core;
using System.Text.Json.Serialization;

namespace EvDb.Scenes;


public record CourseCreated(int Id, string Name, int Capacity) : IEvDbEventPayload
{
    string IEvDbEventPayload.EventType { get; } = "course-created";
}



