// Ignore Spelling: OutboxProducer Channel

using EvDb.Core;

namespace EvDb.StressTests.Outbox;

[EvDbMessageTypes<Message1>]
[EvDbMessageTypes<Message2>]
[EvDbOutbox<DemoStreamFactory, OutboxShards>]
//[EvDbUseOutboxSerialization<AvroSerializer, PrefixSerializer>(EvDbOutboxSerializationMode.Strict)] 
public partial class StressTestOutbox
{
    protected override Shards[] ChannelToShards(Channels outbox) =>

        outbox switch
        {
            Channels.Channel1 => [Shards.Table1],
            Channels.Channel2 => [Shards.Table2],
            _ => [Shards.Table1, Shards.Table2]
        };

    protected override void ProduceOutboxMessages(FaultOccurred payload, IEvDbEventMeta meta, EvDbDemoStreamViews views,
        StressTestOutboxContext outboxs)
    {
        outboxs.Add(new Message1(views.Count), Message1.Channels.Channel1);
        outboxs.Add(new Message2(views.Count), Message2.Channels.Channel3);
        ////if (views.Count % 2 == 0)
        ////{
        ////    outboxs.Add(new Message1(views.Count), Message1.Channels.Channel1);
        ////}
        ////else
        ////{
        ////    outboxs.Add(new Message2(views.Count), Message2.Channels.Channel3);
        ////}
    }
}

