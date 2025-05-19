// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using EvDb.Adapters.Store.MongoDB;
using EvDb.Adapters.Store.Postgres;
using EvDb.Adapters.Store.SqlServer;
using EvDb.Core.Adapters;
using EvDb.Scenes;
using EvDb.UnitTests;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Xunit.Abstractions;

public abstract class StreamBaseTests : BaseIntegrationTests
{
    protected StreamBaseTests(ITestOutputHelper output, StoreType storeType) :
        base(output, storeType)
    {
    }

    protected virtual StudentPassedMessage? DeserializeStudentPassed(EvDbMessageRecord rec)
    {
        Assert.Equal(42, rec.Payload[0]);
        StudentPassedMessage? result = JsonSerializer.Deserialize<StudentPassedMessage>(rec.Payload[1..]);
        return result;
    }

    [Fact]
    public async Task Stream_Outbox_Succeed()
    {
        var streamId = Steps.GenerateStreamId();
        IEvDbSchoolStream stream = await StorageContext
                            .GivenLocalStreamWithPendingEvents(_storeType, TestingStreamStore, streamId: streamId)
                            .WhenStreamIsSavedAsync();

        await ThenStreamSavedWithoutSnapshot();

        async Task ThenStreamSavedWithoutSnapshot()
        {
            Assert.Equal(4, stream.StoredOffset);
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(0, v.StoreOffset));
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(4, v.FoldOffset));

            Assert.Equal(180, stream.Views.ALL.Sum);
            Assert.Equal(3, stream.Views.ALL.Count);
            Assert.Single(stream.Views.StudentStats.Students);

            StudentStats studentStat = stream.Views.StudentStats.Students[0];
            var student = Steps.CreateStudentEntity();
            Assert.Equal(student.Id, studentStat.StudentId);
            Assert.Equal(student.Name, studentStat.StudentName);
            Assert.Equal(180, studentStat.Sum);
            Assert.Equal(3, studentStat.Count);

            ICollection<EvDbMessageRecord> messagingCollection = await GetOutboxAsync(OutboxShards.Messaging).ToEnumerableAsync();
            EvDbMessageRecord[] messaging = messagingCollection!.ToArray();
            Assert.Equal(4, messaging.Length);
            Assert.All(messaging, m => Assert.Equal("student-received-grade", m.EventType));
            Assert.All(messaging, m => Assert.Equal("student-passed", m.MessageType));
            Assert.All(messaging, m => Assert.True(m.Channel == "channel-3" || m.Channel == "channel-2"));

            ICollection<EvDbMessageRecord> messagingVipCollection = await GetOutboxAsync(OutboxShards.MessagingVip).ToEnumerableAsync();
            EvDbMessageRecord[] messagingVip = messagingVipCollection!.ToArray();
            Assert.Equal(2, messagingVip.Length);
            Assert.All(messagingVip, m => Assert.Equal("student-received-grade", m.EventType));
            Assert.All(messagingVip, m => Assert.Equal("student-passed", m.MessageType));
            Assert.All(messagingVip, m => Assert.Equal("channel-3", m.Channel));
            Assert.All(messagingVip, msg =>
            {
                var pass = DeserializeStudentPassed(msg);
                Assert.Equal(2202, pass?.StudentId);
                Assert.Equal("Lora", pass?.Name);
            });

            ICollection<EvDbMessageRecord> commandsCollection = await GetOutboxAsync(OutboxShards.Commands).ToEnumerableAsync();
            EvDbMessageRecord[] commands = commandsCollection!.ToArray();
            Assert.Single(commands);
            Assert.All(commands, m => Assert.Equal("student-received-grade", m.EventType));
            Assert.All(commands, m => Assert.Equal("student-failed", m.MessageType));
            Assert.All(commands, m => Assert.Equal("channel-1", m.Channel));
            Assert.All(commands, msg =>
            {
                Assert.Equal(42, msg.Payload[0]);
                var fail = JsonSerializer.Deserialize<StudentFailedMessage>(msg.Payload[1..]);
                Assert.Equal(2202, fail!.StudentId);
                Assert.Equal("Lora", fail.Name);
            });

            ICollection<EvDbMessageRecord> defaultscommandsCollection = await GetOutboxAsync(EvDbShardName.Default).ToEnumerableAsync();
            EvDbMessageRecord[] defaults = defaultscommandsCollection!.ToArray();
            Assert.Equal(3, defaults.Length);
            Assert.All(defaults, m => Assert.Equal("student-received-grade", m.EventType));
            Assert.All(defaults, m => Assert.Equal("avg", m.MessageType));
            Assert.All(defaults, m => Assert.Equal(EvDbOutboxConstants.DEFAULT_OUTBOX, m.Channel));

            var avg1 = JsonSerializer.Deserialize<AvgMessage>(defaults[0].Payload);
            Assert.Equal(30, avg1!.Avg);
            var avg2 = JsonSerializer.Deserialize<AvgMessage>(defaults[1].Payload);
            Assert.Equal(45, avg2!.Avg);
            var avg3 = JsonSerializer.Deserialize<AvgMessage>(defaults[2].Payload);
            Assert.Equal(60, avg3!.Avg);


            string connectionString = StoreAdapterHelper.GetConnectionString(_storeType);
            IEvDbStorageStreamAdapter adapter = _storeType switch
            {
                StoreType.SqlServer => EvDbSqlServerStorageAdapterFactory.CreateStreamAdapter(_logger, connectionString, StorageContext, []),
                StoreType.Postgres => EvDbPostgresStorageAdapterFactory.CreateStreamAdapter(_logger, connectionString, StorageContext, []),
                StoreType.MongoDB => EvDbMongoDBStorageAdapterFactory.CreateStreamAdapter(_logger, connectionString, StorageContext, []),
                StoreType.Testing => EvDbTestingStorageAdapterFactory.CreateStreamAdapter(StorageContext, TestingStreamStore),
                _ => throw new NotImplementedException()
            };
            var address = new EvDbStreamCursor(stream.StreamAddress);
            var eventsCollection = await adapter.GetEventsAsync(address).ToEnumerableAsync();
            var events = eventsCollection.ToArray();

            var eventsOffsets = events.Select(e => e.StreamCursor.Offset).ToArray();
            Assert.True(eventsOffsets.SequenceEqual([1, 2, 3, 4]));
            for (int i = 1; i <= defaults.Length; i++)
            {
                EvDbMessageRecord item = defaults[i - 1];
                Assert.Equal("student-received-grade", item.EventType);
                var itemOffset = item.Offset;
                Assert.Equal(i + 1, itemOffset);
                Assert.Contains(itemOffset, eventsOffsets);
            }
        }
    }

    [Fact]
    public async Task Stream_WhenStoringWithSnapshotting_Succeed()
    {
        IEvDbSchoolStream stream = await StorageContext
                            .GivenLocalStreamWithPendingEvents(_storeType, TestingStreamStore, 6)
                            .WhenStreamIsSavedAsync();

        ThenStreamSavedWithSnapshot();

        void ThenStreamSavedWithSnapshot()
        {
            Assert.Equal(7, stream.StoredOffset);
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(7, v.StoreOffset));
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(7, v.FoldOffset));

            Assert.Equal(630, stream.Views.ALL.Sum);
            Assert.Equal(6, stream.Views.ALL.Count);
            Assert.Single(stream.Views.StudentStats.Students);

            StudentStats studentStat = stream.Views.StudentStats.Students[0];
            var student = Steps.CreateStudentEntity();
            Assert.Equal(student.Id, studentStat.StudentId);
            Assert.Equal(student.Name, studentStat.StudentName);
            Assert.Equal(630, studentStat.Sum);
            Assert.Equal(6, studentStat.Count);
        }
    }

    [Fact]
    public async Task Stream_WhenStoringWithSnapshottingWhenStoringTwice_Succeed()
    {
        IEvDbSchoolStream stream = await StorageContext
                            .GivenLocalStreamWithPendingEvents(_storeType, TestingStreamStore)
                            .GivenStreamIsSavedAsync()
                            .GivenAddingPendingEventsAsync()
                            .WhenStreamIsSavedAsync();

        ThenStreamSavedWithSnapshot();

        void ThenStreamSavedWithSnapshot()
        {
            Assert.Equal(7, stream.StoredOffset);
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(7, v.StoreOffset));
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(7, v.FoldOffset));

            Assert.Equal(360, stream.Views.ALL.Sum);
            Assert.Equal(6, stream.Views.ALL.Count);
            Assert.Single(stream.Views.StudentStats.Students);

            StudentStats studentStat = stream.Views.StudentStats.Students[0];
            var student = Steps.CreateStudentEntity();
            Assert.Equal(student.Id, studentStat.StudentId);
            Assert.Equal(student.Name, studentStat.StudentName);
            Assert.Equal(360, studentStat.Sum);
            Assert.Equal(6, studentStat.Count);
        }
    }


    [Fact]
    public async Task Stream_WhenStoringStaleStream_ThrowException()
    {
        string streamId = $"occ-{Guid.NewGuid():N}";

        IEvDbSchoolStream stream1 = await StorageContext
                    .GivenLocalStreamWithPendingEvents(_storeType, TestingStreamStore, streamId: streamId);
        IEvDbSchoolStream stream2 = await StorageContext
                    .GivenLocalStreamWithPendingEvents(_storeType, TestingStreamStore, streamId: streamId);

        await Assert.ThrowsAsync<OCCException>(() =>
            Task.WhenAll(
                    stream1.WhenStreamIsSavedAsync(),
                    stream2.WhenStreamIsSavedAsync()
                ));
    }
}