// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using Cocona;
using EvDb.Core.Adapters;
using EvDb.Scenes;
using EvDb.UnitTests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Xunit.Abstractions;

public abstract class StreamSimpleBaseTests : BaseIntegrationTests
{
    private readonly IEvDbSchoolStream _stream;
    protected readonly IConfiguration _configuration;

    protected StreamSimpleBaseTests(ITestOutputHelper output, StoreType storeType) :
        base(output, storeType, true)
    {
        Guid streamId = Guid.NewGuid();
        var builder = CoconaApp.CreateBuilder();
        var services = builder.Services;
        services.AddEvDb()
                .AddSchoolStreamFactory(c => c.ChooseStoreAdapter(storeType, TestingStreamStore), StorageContext)
                .DefaultSnapshotConfiguration(c => c.ChooseSnapshotAdapter(storeType, TestingStreamStore, AlternativeContext));
        var sp = services.BuildServiceProvider();
        _configuration = sp.GetRequiredService<IConfiguration>();
        IEvDbSchoolStreamFactory factory = sp.GetRequiredService<IEvDbSchoolStreamFactory>();
        _stream = factory.Create(streamId);

    }

    [Fact]
    public virtual async Task Stream_Basic_Succeed()
    {
        await ProcuceEventsAsync();

        #region Asserts

        Assert.Equal(4, _stream.StoredOffset);
        Assert.Equal(3, _stream.Views.ALL.Count);
        Assert.Equal(260, _stream.Views.ALL.Sum);
        Assert.Single(_stream.Views.StudentStats.Students);
        StudentStats firstStudent = _stream.Views.StudentStats.Students[0];
        Assert.Equal(10, firstStudent.StudentId);
        Assert.Equal("Mikey", firstStudent.StudentName);
        Assert.Equal(3, firstStudent.Count);
        Assert.Equal(260, firstStudent.Sum);


        ICollection<EvDbMessageRecord> messagingCollection = await GetOutboxAsync(OutboxShards.Messaging).ToEnumerableAsync();
        EvDbMessageRecord[] messaging = messagingCollection!.ToArray();
        Assert.Equal(6, messaging.Length);
        Assert.All(messaging, m => Assert.Equal("student-received-grade", m.EventType));
        Assert.All(messaging, m => Assert.Equal("student-passed", m.MessageType));
        Assert.All(messaging, m => Assert.True(m.Channel == "channel-3" || m.Channel == "channel-2"));

        ICollection<EvDbMessageRecord> messagingVipCollection = await GetOutboxAsync(OutboxShards.MessagingVip).ToEnumerableAsync();
        EvDbMessageRecord[] messagingVip = messagingVipCollection!.ToArray();
        Assert.Equal(3, messagingVip.Length);
        Assert.All(messagingVip, m => Assert.Equal("student-received-grade", m.EventType));
        Assert.All(messagingVip, m => Assert.Equal("student-passed", m.MessageType));
        Assert.All(messagingVip, m => Assert.Equal("channel-3", m.Channel));
        Assert.All(messagingVip, msg =>
        {
            var pass = DeserializeStudentPassed(msg);
            Assert.Equal(10, pass?.StudentId);
            Assert.Equal("Mikey", pass?.Name);
        });

        #endregion //  Asserts
    }

    [Fact]
    public async Task Stream_Basic_With_6_Grades_Succeed_Succeed()
    {
        await ProcuceEventsAsync(6);

        #region Asserts

        Assert.Equal(7, _stream.StoredOffset);
        Assert.Equal(6, _stream.Views.ALL.Count);
        Assert.Equal(510, _stream.Views.ALL.Sum);
        Assert.Single(_stream.Views.StudentStats.Students);
        StudentStats firstStudent = _stream.Views.StudentStats.Students[0];
        Assert.Equal(10, firstStudent.StudentId);
        Assert.Equal("Mikey", firstStudent.StudentName);
        Assert.Equal(6, firstStudent.Count);
        Assert.Equal(510, firstStudent.Sum);

        #endregion //  Asserts
    }


    private async Task ProcuceEventsAsync(int numOfGrades = 3)
    {
        var student = new StudentEntity(10, "Mikey");
        var studentEnlisted = new StudentEnlistedEvent(student);
        await _stream.AddAsync(studentEnlisted);
        for (int i = 1; i <= numOfGrades; i++)
        {
            var grade = new StudentReceivedGradeEvent(i, student.Id, i % 2 == 0 ? 80 : 90);
            await _stream.AddAsync(grade);
        }
        await _stream.StoreAsync();
    }

    private static StudentPassedMessage? DeserializeStudentPassed(EvDbMessageRecord rec)
    {
        Assert.Equal(42, rec.Payload[0]);
        StudentPassedMessage? result = JsonSerializer.Deserialize<StudentPassedMessage>(rec.Payload[1..]);
        return result;
    }
}