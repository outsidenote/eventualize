// Ignore Spelling: Topic

using EvDb.Core;

namespace EvDb.Scenes;

[EvDbAttachTopic("topic-1")]
[EvDbPayload("student-failed")]
public partial record StudentFailedMessage(int StudentId, string Name, DateTimeOffset When, double Grade);




