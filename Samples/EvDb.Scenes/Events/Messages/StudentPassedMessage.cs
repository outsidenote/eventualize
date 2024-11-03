// Ignore Spelling: Topic

using EvDb.Core;

namespace EvDb.Scenes;

// TODO: refactor the outbox structure
//[EvDbOutboxTags]
//public abstract class OutboxTags
//{
//    public static readonly EvDbTableName Topic1 = "topic-1";
//    public static readonly EvDbTableName Topic2 = "topic-2";
//    public static readonly EvDbTableName Topic3 = "topic-3";
//}

//[EvDbAttachToOutbox<OutboxTags>(OutboxTags.Topic1, OutboxTags.Topic2, OutboxTags.Topic3)]


[EvDbAttachToDefaultTopic]
[EvDbAttachTopic("topic-1")]
[EvDbAttachTopic("topic-3")]
[EvDbAttachTopic("topic-2")]
[EvDbDefinePayload("student-passed")]
public partial record StudentPassedMessage(int StudentId, string Name, DateTimeOffset When, double Grade);




