// Ignore Spelling: Topic

using EvDb.Core;

namespace EvDb.Scenes;

[EvDbAttachTopic("topic-4")]
[EvDbAttachTopic("topic-3")]
[EvDbAttachTopic("topic-1")]
[EvDbPayload("student-pass")]
public partial record StudentPassTopic(int StudentId, string Name, DateTimeOffset When, double Grade);




