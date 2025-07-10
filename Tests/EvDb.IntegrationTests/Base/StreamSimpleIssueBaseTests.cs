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

public abstract class StreamSimpleIssueBaseTests : BaseIntegrationTests
{
    private readonly IProblemsFactory _factory;
    private readonly Guid _streamId = Guid.NewGuid();
    private IProblems _stream;
    protected readonly IConfiguration _configuration;

    protected StreamSimpleIssueBaseTests(ITestOutputHelper output, StoreType storeType) :
        base(output, storeType, true)
    {
        var builder = CoconaApp.CreateBuilder();
        var services = builder.Services;
        services.AddEvDb()
                .AddIssueStreamFactory(c => c.ChooseStoreAdapter(storeType, TestingStreamStore), StorageContext)
                .DefaultSnapshotConfiguration(c => c.ChooseSnapshotAdapter(storeType, TestingStreamStore, AlternativeContext));
        var sp = services.BuildServiceProvider();
        _configuration = sp.GetRequiredService<IConfiguration>();
        _factory = sp.GetRequiredService<IProblemsFactory>();
        _stream = _factory.Create(_streamId);
    }

    [Fact]
    public virtual async Task View_Persist_Strategy_Succeed()
    {
        await ProcuceEventsAsync();

        var viewsMeta = _stream.Views.ToTypedMetadata().ShouldSave;
        Assert.Equal(StudentReceivedGradeEvent.PAYLOAD_TYPE, _stream.Views.ShouldSave!.Value.EventType);
        Assert.Equal(viewsMeta.StoreOffset, viewsMeta.MemoryOffset);

        _stream = await _factory.GetAsync(_streamId);
        viewsMeta = _stream.Views.ToTypedMetadata().ShouldSave;
        Assert.Equal(viewsMeta.StoreOffset, viewsMeta.MemoryOffset);

        await ProcuceEventsAsync(false);
        viewsMeta = _stream.Views.ToTypedMetadata().ShouldSave;
        Assert.NotEqual(viewsMeta.StoreOffset, viewsMeta.MemoryOffset);

        await ProcuceEventsAsync(false);
        viewsMeta = _stream.Views.ToTypedMetadata().ShouldSave;
        Assert.Equal(viewsMeta.StoreOffset, viewsMeta.MemoryOffset);
    }

    private async Task ProcuceEventsAsync(bool withRoot = true, int numOfGrades = 3)
    {
        int id = 10;
        if (withRoot)
        {
            var student = new StudentEntity(id, "Mikey");
            var studentEnlisted = new StudentEnlistedEvent(student);
            await _stream.AppendAsync(studentEnlisted);
        }
        for (int i = 1; i <= numOfGrades; i++)
        {
            var grade = new StudentReceivedGradeEvent(i, id, i % 2 == 0 ? 80 : 90);
            await _stream.AppendAsync(grade);
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