// Ignore Spelling: OutboxProducer Channel

using EvDb.Core;

namespace EvDb.StressTestsWebApi.Outbox;

[EvDbAttachMessageType<Message1>]
[EvDbAttachMessageType<Message2>]
[EvDbOutbox<DemoStreamFactory, OutboxShards>]
[EvDbUseOutboxSerialization<EmbeddedSchemaSerializer>(EvDbOutboxSerializationMode.Strict)]
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
        StressTestOutboxContext outbox)
    {
        if (views.Count % 2 == 0)
        {
            outbox.Add(new Message1 { Amount = views.Count }, Message1.Channels.Channel1);
        }
        else
        {
            outbox.Add(new Message2 { Value = views.Count }, Message2.Channels.Channel3);
        }
    }
}

