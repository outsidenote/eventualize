// Ignore Spelling: Outbox

using EvDb.Core;

namespace EvDb.Scenes;

[EvDbPayload("student-fail")]
public partial record StudentFailOutbox(int StudentId, string Name, DateTimeOffset When, double Grade);




