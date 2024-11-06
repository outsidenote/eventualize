using EvDb.Core;

namespace EvDb.UnitTests;

[EvDbOutboxTables]
public abstract class OutboxTables
{
    public static readonly EvDbTableName MessagingVip = "messaging-vip";
    public static readonly EvDbTableName Messaging = "messaging";
    public static readonly EvDbTableName Commands = "commands";
}

