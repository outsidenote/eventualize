// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using Cocona;
using EvDb.Scenes;
using EvDb.UnitTests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Xunit.Abstractions;


[Collection("Stress")]
[Trait("Kind", "Integration:stress")]
public abstract class ChangeStreamStressBaseTests : BaseIntegrationTests
{
    private readonly IEvDbNoViews _stream;
    protected readonly IConfiguration _configuration;
    private readonly IEvDbNoViewsFactory _factory;
    private readonly IEvDbChangeStream _changeStream;
    private readonly Guid _streamId;

    #region Ctor

    protected ChangeStreamStressBaseTests(ITestOutputHelper output, StoreType storeType) :
        base(output, storeType, true)
    {
        var builder = CoconaApp.CreateBuilder();
        var services = builder.Services;

        // Configure logging to use xUnit output
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(new XUnitLoggerProvider(_output));
        builder.Logging.SetMinimumLevel(LogLevel.Debug);

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

    #endregion //  Ctor

    #region ChangeStream_Stress

    [Trait("Kind", "Integration:stress")]
    [Theory(Timeout = 5_000)]
    //[Theory]
    [InlineData(1000, 10, 1)]
    [InlineData(1000, 10, 10)]
    //[InlineData(10_000, 10, 1)]
    //[InlineData(10_000, 10, 20)]
    public virtual async Task ChangeStream_Stress(int totalEvents, int eventChunk, int maxDegreeOfParallelism)
    {
        EvDbShardName shard = EvDbNoViewsOutbox.DEFAULT_SHARD_NAME;

        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        var defaultEventsOptions = EvDbContinuousFetchOptions.ContinueWhenEmpty;
        var startAt = DateTimeOffset.UtcNow.AddMinutes(-1);


        IAsyncEnumerable<ActivityBag<EvDbMessage>> messages =
                _changeStream.GetFromOutboxAsync(shard, startAt, defaultEventsOptions, cancellationToken);

        var block = new ActionBlock<int>(async i =>
        {
            await ProcuceStudentReceivedGradeAsync(eventChunk, i * eventChunk);
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism });

        int iterations = totalEvents / eventChunk;
        for (int j = 0; j < iterations; j++)
        {
            block.Post(j);
        }
        block.Complete();

        int count = 0;
        await foreach (EvDbMessage message in messages)
        {
            count++;
            Assert.Equal(count, message.StreamCursor.Offset);
            if (count % 100 == 0)
            {
                _output.WriteLine($"Processed {count} messages");
            }
            if (count == totalEvents)
                await cts.CancelAsync();
        }
    }

    #endregion //  ChangeStream_Stress

    #region ProcuceStudentReceivedGradeAsync

    private async Task ProcuceStudentReceivedGradeAsync(int numOfGrades = 3, int seed = 0)
    {
        for (int i = 1; i <= numOfGrades; i++)
        {
            var grade = new StudentReceivedGradeEvent(i, 88, i + seed);
            await _stream.AppendAsync(grade);
        }
        await _stream.StoreAsync();
    }

    #endregion //  ProcuceStudentReceivedGradeAsync
}