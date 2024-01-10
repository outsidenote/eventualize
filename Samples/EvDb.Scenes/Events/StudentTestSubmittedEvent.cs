using EvDb.Core;
using System.Text.Json;

namespace EvDb.Scenes;

public record StudentTestSubmittedEvent(int TestId, JsonElement data) : IEvDbEventPayload
{
    string IEvDbEventPayload.EventType { get; } = "student-test-submitted";
}

