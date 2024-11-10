// Ignore Spelling: Channel

using EvDb.Core;

namespace EvDb.Scenes;

// TODO: refactor the outbox structure
//[EvDbOutboxTags]
//public abstract class OutboxTags
//{
//    public static readonly EvDbShardName Topic1 = "channel-1";
//    public static readonly EvDbShardName Topic2 = "channel-2";
//    public static readonly EvDbShardName Topic3 = "channel-3";
//}

//[EvDbAttachToOutbox<OutboxTags>(OutboxTags.Topic1, OutboxTags.Topic2, OutboxTags.Topic3)]


[EvDbAttachDefaultChannel]
[EvDbAttachChannel("channel-1")]
[EvDbAttachChannel("channel-3")]
[EvDbAttachChannel("channel-2")]
[EvDbDefinePayload("student-passed")]
public partial record StudentPassedMessage(int StudentId, string Name, DateTimeOffset When, double Grade);




