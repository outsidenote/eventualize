using EvDb.Core;

namespace EvDb.StressTestsWebApi.Outbox;

// TODO: [bnaya 2024-11-07] Consider how to expose it as a const (for pattern matching keys)
[EvDbOutboxShards]
public abstract class OutboxShards
{
    public static readonly EvDbShardName Table1 = "table1";
    public static readonly EvDbShardName Table2 = "table2";
}

