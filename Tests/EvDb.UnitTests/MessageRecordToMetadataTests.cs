// Ignore Spelling: Bson

using EvDb.Adapters.Store.Internals;
using EvDb.Core;
using EvDb.Core.Adapters;
using OpenTelemetry;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Text;
using Xunit.Abstractions;
using static EvDb.Core.Adapters.Internals.EvDbStoreNames;

namespace EvDb.UnitTests;

[Trait("Kind", "UnitTest")]
public sealed class MessageRecordToMetadataTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly ActivitySource TraceSource = new ActivitySource("Test");
    private readonly TracerProvider _tracerProvider;

    public MessageRecordToMetadataTests(ITestOutputHelper output)
    {
        _output = output;

        _tracerProvider = Sdk.CreateTracerProviderBuilder()
                 .SetSampler<AlwaysOnSampler>()
                 .AddSource(TraceSource.Name)
                 .Build();
    }

    [Fact]
    public void Test_With_Activity()
    {
        using var activity = TraceSource.StartActivity("test-scope");

        Assert.NotNull(activity);
        Assert.Equal(activity, Activity.Current);
    }

    [Fact]
    public void MessageRecordToMetadata()
    {
        using var activity = TraceSource.StartActivity("test-scope");

        var messageRecord = new EvDbMessageRecord
        {
            StreamType = "TestDomain:TestPartition",
            StreamId = "TestStreamId",
            Offset = 1,
            EventType = "TestEventType",
            Channel = "TestChannel",
            MessageType = "TestMessageType",
            SerializeType = "TestSerializeType",
            Payload = Encoding.UTF8.GetBytes("TestPayload"),
            CapturedBy = "TestCapturedBy",
            CapturedAt = DateTimeOffset.UtcNow,
            TraceParent = activity?.SerializeTelemetryContext() ?? EvDbOtelTraceParent.Empty
        };

        IEvDbEventMeta meta = messageRecord.GetMetadata();

        _output.WriteLine($"MessageRecord: {messageRecord}");

        Assert.Equal(messageRecord.StreamType, meta.StreamCursor.StreamType);
        Assert.Equal(messageRecord.StreamId, meta.StreamCursor.StreamId);
        Assert.Equal(messageRecord.Offset, meta.StreamCursor.Offset);
        Assert.Equal(messageRecord.EventType, meta.EventType);
        Assert.Equal(messageRecord.CapturedAt, meta.CapturedAt);
        Assert.Equal(messageRecord.CapturedBy, meta.CapturedBy);
    }


    [Fact]
    public void BsonMessageRecordToMetadata()
    {
        using var activity = TraceSource.StartActivity("test-scope");

        var messageRecord = new EvDbMessageRecord
        {
            StreamType = "TestDomain:TestPartition",
            StreamId = "TestStreamId",
            Offset = 1,
            EventType = "TestEventType",
            Channel = "TestChannel",
            MessageType = "TestMessageType",
            SerializeType = "TestSerializeType",
            Payload = Encoding.UTF8.GetBytes("TestPayload"),
            CapturedBy = "TestCapturedBy",
            CapturedAt = DateTimeOffset.UtcNow,
            TraceParent = activity?.SerializeTelemetryContext() ?? EvDbOtelTraceParent.Empty
        };

        var doc = messageRecord.EvDbToBsonDocument("shard");
        var capturedAt = doc.GetValue(Fields.Message.CapturedAt);
        doc.Add(Fields.Message.StoredAt, capturedAt); // happened at the DB level
        var meta = doc.ToMessageMeta();

        _output.WriteLine($"MessageRecord: {messageRecord}");

        Assert.Equal(messageRecord.StreamType, meta.StreamCursor.StreamType);
        Assert.Equal(messageRecord.StreamId, meta.StreamCursor.StreamId);
        Assert.Equal(messageRecord.Offset, meta.StreamCursor.Offset);
        Assert.Equal(messageRecord.EventType, meta.EventType);
        Assert.Equal(messageRecord.Channel, meta.Channel);
        Assert.Equal(messageRecord.CapturedBy, meta.CapturedBy);
        messageRecord.AssertTelemetryContextEquals(meta);
    }

    [Fact]
    public void BsonMessageRecordToMetadataFromActivityTelemetry()
    {
        var activity = new Activity("test-scope");
        activity.Start();

        var messageRecord = new EvDbMessageRecord
        {
            StreamType = "TestDomain:TestPartition",
            StreamId = "TestStreamId",
            Offset = 1,
            EventType = "TestEventType",
            Channel = "TestChannel",
            MessageType = "TestMessageType",
            SerializeType = "TestSerializeType",
            Payload = Encoding.UTF8.GetBytes("TestPayload"),
            CapturedBy = "TestCapturedBy",
            CapturedAt = DateTimeOffset.UtcNow,
            StoredAt = DateTimeOffset.UtcNow,
        };

        var doc = messageRecord.EvDbToBsonDocument("shard");
        var capturedAt = doc.GetValue(Fields.Message.CapturedAt);
        doc.Add(Fields.Message.StoredAt, capturedAt); // happened at the DB level
        var meta = doc.ToMessageMeta();

        _output.WriteLine($"MessageRecord: {messageRecord}");

        Assert.Equal(messageRecord.StreamType, meta.StreamCursor.StreamType);
        Assert.Equal(messageRecord.StreamId, meta.StreamCursor.StreamId);
        Assert.Equal(messageRecord.Offset, meta.StreamCursor.Offset);
        Assert.Equal(messageRecord.EventType, meta.EventType);
        Assert.Equal(messageRecord.Channel, meta.Channel);
        Assert.Equal(messageRecord.CapturedBy, meta.CapturedBy);
        Assert.NotNull(messageRecord.StoredAt);

        EvDbOtelTraceParent currentOtelBuffer = Activity.Current?.SerializeTelemetryContext() ?? EvDbOtelTraceParent.Empty;
        meta.AssertTelemetryContextEquals(currentOtelBuffer);
    }

    public void Dispose()
    {
        _tracerProvider.Dispose();
    }
}
