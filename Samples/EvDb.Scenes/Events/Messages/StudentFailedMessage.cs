// Ignore Spelling: Channel

using EvDb.Core;

namespace EvDb.Scenes;

[EvDbAttachChannel("channel-1")]
[EvDbDefinePayload("student-failed")]
public partial record StudentFailedMessage(int StudentId, string Name, DateTimeOffset When, double Grade);




