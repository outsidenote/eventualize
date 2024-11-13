using EvDb.Core;
using System.Text.Json;

namespace EvDb.Scenes;

[EvDbDefineEventPayload("student-test-submitted")]
public partial record StudentTestSubmittedEvent(int TestId, JsonElement data);

