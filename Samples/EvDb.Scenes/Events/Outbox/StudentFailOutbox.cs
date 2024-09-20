// Ignore Spelling: Topic

using EvDb.Core;

namespace EvDb.Scenes;

[EvDbAttachTopic("topic-2")]
[EvDbAttachTopic("topic-1")]
[EvDbPayload("student-fail")]
public partial record StudentFailTopic(int StudentId, string Name, DateTimeOffset When, double Grade);




