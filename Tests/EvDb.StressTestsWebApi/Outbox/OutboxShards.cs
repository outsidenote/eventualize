using EvDb.Core;

namespace EvDb.StressTestsWebApi.Outbox;

[EvDbOutboxShards]
public abstract class OutboxShards
{
    public static readonly EvDbShardName Table1 = "table1";
    public static readonly EvDbShardName Table2 = "table2";
}

