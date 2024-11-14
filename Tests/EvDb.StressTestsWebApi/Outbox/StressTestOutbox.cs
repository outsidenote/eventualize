// Ignore Spelling: OutboxProducer Channel

using EvDb.Core;

namespace EvDb.StressTestsWebApi.Outbox;

[EvDbMessageTypes<Message1>]
[EvDbMessageTypes<Message2>]
[EvDbOutbox<DemoStreamFactory, OutboxShards>]
//[EvDbUseOutboxSerialization<AvroSerializer, PrefixSerializer>(EvDbOutboxSerializationMode.Strict)] 
public partial class StressTestOutbox // TODO: MessageRouter / Outbox
{
    protected override OutboxShardsPreferences[] ChannelToShards(StressTestOutboxChannels outbox) =>

        outbox switch
        {
            StressTestOutboxChannels.Channel1 => [OutboxShardsPreferences.Table1],
            StressTestOutboxChannels.Channel2 => [OutboxShardsPreferences.Table2],
            _ => [OutboxShardsPreferences.Table1, OutboxShardsPreferences.Table2]
        };

    protected override void ProduceOutboxMessages(FaultOccurred payload, IEvDbEventMeta meta, EvDbDemoStreamViews views,
        StressTestOutboxContext outboxs)
    {
        if (views.Count % 2 == 0)
        {
            outboxs.Add(new Message1(views.Count), OutboxOfMessage1.Channel1);
        }
        else
        {
            outboxs.Add(new Message2(views.Count), OutboxOfMessage2.Channel3);
        }
    }
}

