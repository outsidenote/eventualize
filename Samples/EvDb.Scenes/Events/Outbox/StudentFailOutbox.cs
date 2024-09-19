// Ignore Spelling: Topic

using EvDb.Core;

namespace EvDb.Scenes;

[EvDbPayload("student-fail")]
public partial record StudentFailTopic(int StudentId, string Name, DateTimeOffset When, double Grade);




