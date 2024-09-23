using EvDb.Core;
using System.Text.Json;

namespace EvDb.Scenes;

[EvDbDefinePayload("student-test-submitted")]
public partial record StudentTestSubmittedEvent(int TestId, JsonElement data);

