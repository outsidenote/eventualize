// Ignore Spelling: Channel

using EvDb.Core;
using EvDb.UnitTests;

namespace EvDb.Scenes;

[EvDbAttachChannel(OutboxChannels.Channel1)]
[EvDbDefineMessagePayload("student-failed")]
public partial record StudentFailedMessage(int StudentId, string Name, DateTimeOffset When, double Grade);




