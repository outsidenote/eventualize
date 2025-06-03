// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using Cocona;
using EvDb.Core.Adapters;
using EvDb.Scenes;
using EvDb.UnitTests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Text.Json;
using Xunit.Abstractions;

public abstract class StreamNoViewsBaseTests : BaseIntegrationTests
{
    private readonly IEvDbNoViews _stream;
    protected readonly IConfiguration _configuration;
    private readonly IEvDbNoViewsFactory _factory;
    private readonly IEvDbChangeStream _changeStream;
    private readonly Guid _streamId;

    protected StreamNoViewsBaseTests(ITestOutputHelper output, StoreType storeType) :
        base(output, storeType, true)
    {
        var builder = CoconaApp.CreateBuilder();
        var services = builder.Services;
        services.AddEvDb()
                .AddNoViewsFactory(c => c.ChooseStoreAdapter(storeType, TestingStreamStore), StorageContext)
                .DefaultSnapshotConfiguration(c => c.ChooseSnapshotAdapter(storeType, TestingStreamStore, AlternativeContext));
        services.AddEvDb()
                .AddChangeStream(storeType, StorageContext);

        var sp = services.BuildServiceProvider();
        _configuration = sp.GetRequiredService<IConfiguration>();
        _factory = sp.GetRequiredService<IEvDbNoViewsFactory>();
        _streamId = Guid.NewGuid();
        _stream = _factory.Create(_streamId);
        _changeStream = sp.GetRequiredService<IEvDbChangeStream>();
    }

    #region Stream_NoView_Succeed

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

    #endregion //  Stream_NoView_Succeed

    #region Stream_NoView_BeyondBatchSize_Succeed

    [Fact]
    public virtual async Task Stream_NoView_BeyondBatchSize_Succeed()
    {
        var defaultEventsOptions = EvDbContinuousFetchOptions.CompleteIfEmpty;
        int count = defaultEventsOptions.BatchSize * 2;

        await ProcuceEventsAsync(count);

        #region Asserts

        Assert.Equal(count + 1, _stream.StoredOffset);


        ICollection<EvDbMessageRecord> messagingCollection = await GetOutboxAsync(EvDbNoViewsOutbox.DEFAULT_SHARD_NAME).ToEnumerableAsync();
        EvDbMessageRecord[] messaging = messagingCollection!.ToArray();
        Assert.Equal(count, messaging.Length);

        #endregion //  Asserts

        IEvDbNoViews stream = await _factory.GetAsync(_streamId);

        #region Asserts

        Assert.Equal(count + 1, stream.StoredOffset);

        #endregion //  Asserts
    }

    #endregion //  Stream_NoView_BeyondBatchSize_Succeed

    #region Stream_NoViewEmpty_Succeed

    [Fact]
    public virtual async Task Stream_NoViewEmpty_Succeed()
    {
        Assert.Equal(0, _stream.StoredOffset);

        IEvDbNoViews stream = await _factory.GetAsync(_streamId);

        Assert.Equal(0, stream.StoredOffset);
    }

    #endregion //  Stream_NoViewEmpty_Succeed

    #region Stream_NoView_GetMessages_Succeed

    [Fact]
    public virtual async Task Stream_NoView_GetMessages_Succeed()
    {
        var cancellationDucraion = Debugger.IsAttached 
                                        ? TimeSpan.FromMinutes(10)
                                        : TimeSpan.FromSeconds(5);

        using var cts = new CancellationTokenSource(cancellationDucraion);
        var cancellationToken = cts.Token;
        var defaultEventsOptions = EvDbContinuousFetchOptions.CompleteIfEmpty;
        int count = defaultEventsOptions.BatchSize * 2;

        // produce messages before start listening to the change stream
        await ProcuceEventsAsync(count);

        var startAt = DateTimeOffset.UtcNow.AddSeconds(-1);
        EvDbShardName shard = EvDbNoViewsOutbox.DEFAULT_SHARD_NAME;
        IAsyncEnumerable<EvDbMessage> messages = 
                        _changeStream.GetMessagesAsync(shard, startAt, defaultEventsOptions, cancellationToken);

        long lastOffset = 1;
        await foreach (var message in messages)
        {
            long messageOffset = message.StreamCursor.Offset;
            Assert.Equal(lastOffset, messageOffset - 1);
            lastOffset = messageOffset;

            AvgMessage data = JsonSerializer.Deserialize<AvgMessage>(message.Payload);
            Assert.Equal(messageOffset % 2 == 0 ? 90 : 80, data.Avg);

            if (messageOffset == defaultEventsOptions.BatchSize)
            {
                var _ = ProcuceEventsAsync(count); // produce more messages after start listening to the change stream
            }
            if (messageOffset == count * 2 + 1)
                await cts.CancelAsync();
        }

        Assert.Equal(count * 2 + 1, _stream.StoredOffset);
    }

    #endregion //  Stream_NoView_GetMessages_Succeed

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