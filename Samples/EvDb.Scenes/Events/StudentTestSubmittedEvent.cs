using EvDb.Core;
using System.Text.Json;

namespace EvDb.Scenes;

[EvDbEventPayload("student-test-submitted")]
public partial record StudentTestSubmittedEvent(int TestId, JsonElement data);

