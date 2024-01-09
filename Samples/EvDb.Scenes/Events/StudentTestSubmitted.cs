using EvDb.Core;
using System.Text.Json;

namespace EvDb.Scenes;

public record StudentTestSubmitted(int TestId, JsonElement data) : IEvDbEventPayload
{
    string IEvDbEventPayload.EventType { get; } = "student-test-submitted";
}

