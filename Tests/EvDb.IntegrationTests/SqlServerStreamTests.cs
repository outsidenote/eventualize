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

            var avg1 = JsonSerializer.Deserialize<AvgTopic>(messages[0].Payload);
            Assert.Equal(30, avg1!.Avg);           
            var avg2 = JsonSerializer.Deserialize<AvgTopic>(messages[2].Payload);
            Assert.Equal(45, avg2!.Avg);
            var avg3 = JsonSerializer.Deserialize<AvgTopic>(messages[4].Payload);
            Assert.Equal(60, avg3!.Avg);

            var eventsOffsets = events.Select(e => e.StreamCursor.Offset).ToArray();
            for (int i = 0; i < messages.Length; i++)
            {
                var item = messages[i];
                Assert.Equal("student-received-grade", item.EventType);
                var itemOffset = item.StreamCursor.Offset;
                Assert.Equal(i / 2 + 1, itemOffset);
                Assert.Contains(itemOffset, eventsOffsets);
            }

            // Avg
            for (int i = 0; i < 6; i+=2)
            {
                Assert.Equal("avg", messages[i].MessageType);
            }

            // Fail
            Assert.Equal("student-fail", messages[1].MessageType);
            var fail = JsonSerializer.Deserialize<StudentFailTopic>(messages[1].Payload);
            Assert.Equal(2202, fail.StudentId);
            Assert.Equal("Lora", fail.Name);

            // Pass
            for (int i = 3; i < 6; i+=2)
            {
                Assert.Equal("student-pass", messages[i].MessageType);
                var pass = JsonSerializer.Deserialize<StudentPassTopic>(messages[i].Payload);
                Assert.Equal(2202, pass.StudentId);
                Assert.Equal("Lora", pass.Name);
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