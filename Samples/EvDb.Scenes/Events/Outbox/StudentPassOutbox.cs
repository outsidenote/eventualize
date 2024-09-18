// Ignore Spelling: Outbox

using EvDb.Core;

namespace EvDb.Scenes;

[EvDbPayload("student-pass")]
public partial record StudentPassOutbox(int StudentId, string Name, DateTimeOffset When, double Grade);




