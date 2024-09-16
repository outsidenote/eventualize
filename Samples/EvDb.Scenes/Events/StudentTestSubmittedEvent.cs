using EvDb.Core;
using System.Text.Json;

namespace EvDb.Scenes;

[EvDbPayload("student-test-submitted")]
public partial record StudentTestSubmittedEvent(int TestId, JsonElement data);

