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

public abstract class StreamNoViewsBaseTests : BaseIntegrationTests
{
    private readonly IEvDbNoViews _stream;
    protected readonly IConfiguration _configuration;
    private readonly IEvDbNoViewsFactory _factory;
    private readonly Guid _streamId;

    protected StreamNoViewsBaseTests(ITestOutputHelper output, StoreType storeType) :
        base(output, storeType, true)
    {
        var builder = CoconaApp.CreateBuilder();
        var services = builder.Services;
        services.AddEvDb()
                .AddNoViewsFactory(c => c.ChooseStoreAdapter(storeType, TestingStreamStore), StorageContext)
                .DefaultSnapshotConfiguration(c => c.ChooseSnapshotAdapter(storeType, TestingStreamStore, AlternativeContext));
        var sp = services.BuildServiceProvider();
        _configuration = sp.GetRequiredService<IConfiguration>();
        _factory = sp.GetRequiredService<IEvDbNoViewsFactory>();
        _streamId = Guid.NewGuid();
        _stream = _factory.Create(_streamId);
    }

    [Fact]
    public virtual async Task Stream_NoView_Succeed()
    {
        await ProcuceEventsAsync();

        #region Asserts

        Assert.Equal(4, _stream.StoredOffset);


        ICollection<EvDbMessageRecord> messagingCollection = await GetOutboxAsync(EvDbNoViewsOutbox.DEFAULT_SHARD_NAME).ToEnumerableAsync();
        EvDbMessageRecord[] messaging = messagingCollection!.ToArray();
        Assert.Equal(3, messaging.Length);

        #endregion //  Asserts

        IEvDbNoViews stream = await _factory.GetAsync(_streamId);

        #region Asserts

        Assert.Equal(4, stream.StoredOffset);

        #endregion //  Asserts
    }

    [Fact]
    public virtual async Task Stream_NoViewEmpty_Succeed()
    {
        Assert.Equal(0, _stream.StoredOffset);

        IEvDbNoViews stream = await _factory.GetAsync(_streamId);

        Assert.Equal(0, stream.StoredOffset);
    }

    private async Task ProcuceEventsAsync(int numOfGrades = 3)
    {
        var student = new StudentEntity(10, "Mikey");
        var studentEnlisted = new StudentEnlistedEvent(student);
        await _stream.AppendAsync(studentEnlisted);
        for (int i = 1; i <= numOfGrades; i++)
        {
            var grade = new StudentReceivedGradeEvent(i, student.Id, i % 2 == 0 ? 80 : 90);
            await _stream.AppendAsync(grade);
        }
        await _stream.StoreAsync();
    }
}