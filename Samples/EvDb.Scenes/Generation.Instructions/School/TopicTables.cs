using EvDb.Core;

namespace EvDb.UnitTests;

[EvDbTopicTables]
public abstract class TopicTables
{
    public static readonly EvDbTableName MessagingVip = "messaging-vip";
    public static readonly EvDbTableName Messaging = "messaging";
    public static readonly EvDbTableName Commands = "commands";
}

