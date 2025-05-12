using EvDb.Core;
using EvDb.Core.Adapters;
using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using EvDb.Adapters.Store.Internals;

namespace EvDb.UnitTests;

public class MessageRecordToMetadataTests
{
    private readonly ITestOutputHelper _output;

    public MessageRecordToMetadataTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void MessageRecordToMetadata()
    {
        var messageRecord = new EvDbMessageRecord
        {
            Domain = "TestDomain",
            Partition = "TestPartition",
            StreamId = "TestStreamId",
            Offset = 1,
            EventType = "TestEventType",
            Channel = "TestChannel",
            MessageType = "TestMessageType",
            SerializeType = "TestSerializeType",
            Payload = Encoding.UTF8.GetBytes("TestPayload"),
            CapturedBy = "TestCapturedBy",
            CapturedAt = DateTimeOffset.UtcNow,
            SpanId = "1234",
            TraceId = "4567"
        };

        IEvDbEventMeta meta = messageRecord.GetMetadata();

        _output.WriteLine($"MessageRecord: {messageRecord}");

        Assert.Equal(messageRecord.Domain, meta.StreamCursor.Domain);
        Assert.Equal(messageRecord.Partition, meta.StreamCursor.Partition);
        Assert.Equal(messageRecord.StreamId, meta.StreamCursor.StreamId);
        Assert.Equal(messageRecord.Offset, meta.StreamCursor.Offset);
        Assert.Equal(messageRecord.EventType, meta.EventType);
        Assert.Equal(messageRecord.CapturedAt, meta.CapturedAt);
        Assert.Equal(messageRecord.CapturedBy, meta.CapturedBy);
    }


    [Fact]
    public void BsonMessageRecordToMetadata()
    {
        var messageRecord = new EvDbMessageRecord
        {
            Domain = "TestDomain",
            Partition = "TestPartition",
            StreamId = "TestStreamId",
            Offset = 1,
            EventType = "TestEventType",
            Channel = "TestChannel",
            MessageType = "TestMessageType",
            SerializeType = "TestSerializeType",
            Payload = Encoding.UTF8.GetBytes("TestPayload"),
            CapturedBy = "TestCapturedBy",
            CapturedAt = DateTimeOffset.UtcNow,
            SpanId = "1234",
            TraceId = "4567"
        };

        var doc = messageRecord.EvDbToBsonDocument("shard");
        var meta = doc.ToMessageMeta();

        _output.WriteLine($"MessageRecord: {messageRecord}");

        Assert.Equal(messageRecord.Domain, meta.StreamCursor.Domain);
        Assert.Equal(messageRecord.Partition, meta.StreamCursor.Partition);
        Assert.Equal(messageRecord.StreamId, meta.StreamCursor.StreamId);
        Assert.Equal(messageRecord.Offset, meta.StreamCursor.Offset);
        Assert.Equal(messageRecord.EventType, meta.EventType);
        Assert.Equal(messageRecord.Channel, meta.Channel);
        Assert.Equal(messageRecord.TraceId, meta.TraceId);
        Assert.Equal(messageRecord.SpanId, meta.SpanId);
        Assert.Equal(messageRecord.CapturedBy, meta.CapturedBy);
    }

}
