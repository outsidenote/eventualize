// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using EvDb.Scenes;
using EvDb.UnitTests;
using System.Text.Json;
using Xunit.Abstractions;
using static EvDb.Adapters.Store.SqlServer.EvDbSqlServerStorageAdapterFactory;

public class SqlServerStreamTests : IntegrationTests
{
    public SqlServerStreamTests(ITestOutputHelper output) :
        base(output, StoreType.SqlServer)
    {
    }

    [Fact]
    public async Task Stream_WhenStoringWithoutSnapshotting_Succeed()
    {
        var streamId = Steps.GenerateStreamId();
        IEvDbSchoolStream stream = await StorageContext
                            .GivenLocalStreamWithPendingEvents(streamId: streamId)
                            .WhenStreamIsSavedAsync();

        await ThenStreamSavedWithoutSnapshot();

        async Task ThenStreamSavedWithoutSnapshot()
        {
            Assert.Equal(3, stream.StoredOffset);
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(-1, v.StoreOffset));
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(3, v.FoldOffset));

            Assert.Equal(180, stream.Views.ALL.Sum);
            Assert.Equal(3, stream.Views.ALL.Count);
            Assert.Single(stream.Views.StudentStats.Students);

            StudentStats studentStat = stream.Views.StudentStats.Students[0];
            var student = Steps.CreateStudentEntity();
            Assert.Equal(student.Id, studentStat.StudentId);
            Assert.Equal(student.Name, studentStat.StudentName);
            Assert.Equal(180, studentStat.Sum);
            Assert.Equal(3, studentStat.Count);

            var messageCollection = await GetMessagesFromTopicsAsync().ToEnumerableAsync();
            var messages = messageCollection.ToArray();

            string connectionString = StoreAdapterHelper.GetConnectionString(StoreType.SqlServer);
            IEvDbStorageStreamAdapter adapter = CreateStreamAdapter(_logger, connectionString, StorageContext);
            var address = new EvDbStreamCursor(stream.StreamAddress);
            var eventsCollection = await adapter.GetEventsAsync(address).ToEnumerableAsync();
            var events = eventsCollection.ToArray();

            var avg1 = JsonSerializer.Deserialize<AvgMessage>(messages[0].Payload);
            Assert.Equal(30, avg1!.Avg);           
            var avg2 = JsonSerializer.Deserialize<AvgMessage>(messages[2].Payload);
            Assert.Equal(45, avg2!.Avg);
            var avg3 = JsonSerializer.Deserialize<AvgMessage>(messages[5].Payload);
            Assert.Equal(60, avg3!.Avg);

            const int firstGradeOffset = 1; // it's start from the second event
            var eventsOffsets = events.Select(e => e.StreamCursor.Offset).ToArray();
            for (int i = 0; i < messages.Length; i++)
            {
                var item = messages[i];
                Assert.Equal("student-received-grade", item.EventType);
                var itemOffset = item.StreamCursor.Offset;
                int expectedOffset = i switch
                {
                    < 2 => firstGradeOffset,            // produce 2 messages
                    _ => (i + 1) / 3 + firstGradeOffset // produce 3 messages
                };
                Assert.Equal(expectedOffset, itemOffset);
                Assert.Contains(itemOffset, eventsOffsets);
            }

            // Avg
             Assert.Equal("avg", messages[0].MessageType);

            // Avg
            for (int i = 2; i < 8; i+=3)
            {
                Assert.Equal("avg", messages[i].MessageType);
            }

            // Fail
            var msg = messages[1];
            Assert.Equal("student-failed", msg.MessageType);
            var fail = JsonSerializer.Deserialize<StudentFailedMessage>(messages[1].Payload);
            Assert.Equal(2202, fail.StudentId);
            Assert.Equal("Lora", fail.Name);
            Assert.Equal("topic-1", msg.Topic);

            // Pass
            for (int i = 3; i < 6; i+=3)
            {
                msg = messages[i];
                Assert.Equal("student-passed", msg.MessageType);
                var pass = JsonSerializer.Deserialize<StudentPassedMessage>(msg.Payload);
                Assert.Equal(2202, pass.StudentId);
                Assert.Equal("Lora", pass.Name);
                Assert.Equal("topic-1", msg.Topic);

                msg = messages[i + 1];
                Assert.Equal("student-passed", msg.MessageType);
                pass = JsonSerializer.Deserialize<StudentPassedMessage>(messages[i+1].Payload);
                Assert.Equal(2202, pass.StudentId);
                Assert.Equal("Lora", pass.Name);
                Assert.Equal("topic-3", msg.Topic);
            }
        }
    }

    [Fact]
    public async Task Stream_WhenStoringWithSnapshotting_Succeed()
    {
        IEvDbSchoolStream stream = await StorageContext
                            .GivenLocalStreamWithPendingEvents(6)
                            .WhenStreamIsSavedAsync();

        ThenStreamSavedWithSnapshot();

        void ThenStreamSavedWithSnapshot()
        {
            Assert.Equal(6, stream.StoredOffset);
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(6, v.StoreOffset));
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(6, v.FoldOffset));

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
                            .GivenLocalStreamWithPendingEvents()
                            .GivenStreamIsSavedAsync()
                            .GivenAddingPendingEventsAsync()
                            .WhenStreamIsSavedAsync();

        ThenStreamSavedWithSnapshot();

        void ThenStreamSavedWithSnapshot()
        {
            Assert.Equal(6, stream.StoredOffset);
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(6, v.StoreOffset));
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(6, v.FoldOffset));

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
                    .GivenLocalStreamWithPendingEvents(streamId: streamId);
        IEvDbSchoolStream stream2 = await StorageContext
                    .GivenLocalStreamWithPendingEvents(streamId: streamId);

        await Assert.ThrowsAsync<OCCException>(() =>
            Task.WhenAll(
                    stream1.WhenStreamIsSavedAsync(),
                    stream2.WhenStreamIsSavedAsync()
                ));
    }
}