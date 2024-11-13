// Ignore Spelling: Channel

using EvDb.Core;

namespace EvDb.Scenes;


//[EvDbAttachToOutbox<OutboxTags>(OutboxChannels.Topic1, OutboxChannels.Topic2, OutboxChannels.Topic3)]


[EvDbAttachDefaultChannel]
[EvDbAttachChannel("channel-1")]
[EvDbAttachChannel("channel-3")]
[EvDbAttachChannel("channel-2")]
[EvDbDefineMessagePayload("student-passed")]
public partial record StudentPassedMessage(int StudentId, string Name, DateTimeOffset When, double Grade);




