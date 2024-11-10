using EvDb.Core;

namespace EvDb.UnitTests;

// TODO: [bnaya 2024-11-07] Consider how to expose it as a const (for pattern matching keys)
[EvDbOutboxShards]
public abstract class OutboxShards
{
    public static readonly EvDbShardName MessagingVip = "messaging-vip";
    public static readonly EvDbShardName Messaging = "messaging";
    public static readonly EvDbShardName Commands = "commands";
}

