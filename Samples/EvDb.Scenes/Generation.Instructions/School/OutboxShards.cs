using EvDb.Core;

namespace EvDb.UnitTests;

[EvDbOutboxShards]
public abstract class OutboxShards
{
    public static readonly EvDbShardName MessagingVip = "messaging-vip";
    public static readonly EvDbShardName Messaging = "messaging";
    public static readonly EvDbShardName Commands = "commands";
}

