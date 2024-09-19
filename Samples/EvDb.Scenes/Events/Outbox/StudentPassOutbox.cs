// Ignore Spelling: Topic

using EvDb.Core;

namespace EvDb.Scenes;

[EvDbPayload("student-pass")]
public partial record StudentPassTopic(int StudentId, string Name, DateTimeOffset When, double Grade);




