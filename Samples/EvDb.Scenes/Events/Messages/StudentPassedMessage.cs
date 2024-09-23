// Ignore Spelling: Topic

using EvDb.Core;

namespace EvDb.Scenes;

[EvDbAttachToDefaultTopic]
[EvDbAttachTopic("topic-3")]
[EvDbAttachTopic("topic-2")]
[EvDbAttachTopic("topic-1")]
[EvDbDefinePayload("student-passed")]
public partial record StudentPassedMessage(int StudentId, string Name, DateTimeOffset When, double Grade);




