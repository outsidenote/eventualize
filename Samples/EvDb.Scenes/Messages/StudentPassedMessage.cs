// Ignore Spelling: Channel

using EvDb.Core;
using EvDb.UnitTests;

namespace EvDb.Scenes;


//[EvDbAttachToOutbox<OutboxTags>(OutboxChannels.Topic1, OutboxChannels.Topic2, OutboxChannels.Topic3)]


[EvDbAttachDefaultChannel]
[EvDbAttachChannel(OutboxChannels.Channel1)]
[EvDbAttachChannel(OutboxChannels.Channel3)]
[EvDbAttachChannel(OutboxChannels.Channel2)]
[EvDbDefineMessagePayload("student-passed")]
public partial record StudentPassedMessage(int StudentId, string Name, DateTimeOffset When, double Grade);




