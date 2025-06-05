// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using Cocona;
using EvDb.Scenes;
using EvDb.UnitTests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;
using Xunit.Abstractions;

public abstract class ChangeStreamBaseTests : BaseIntegrationTests
{
    private readonly IEvDbNoViews _stream;
    protected readonly IConfiguration _configuration;
    private readonly IEvDbNoViewsFactory _factory;
    private readonly IEvDbChangeStream _changeStream;
    private readonly Guid _streamId;

    protected ChangeStreamBaseTests(ITestOutputHelper output, StoreType storeType) :
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

    #region ChangeStream_GetMessages_Succeed

    [Fact]
    public virtual async Task ChangeStream_GetMessages_Succeed()
    {
        const int BATCH_SIZE = 300;
        const int FUTURE_COUNT = 30;
        const int CHUNCK_SIZE = 40;
        var cancellationDucraion = Debugger.IsAttached
                                        ? TimeSpan.FromMinutes(10)
                                        : TimeSpan.FromSeconds(5);

        using var cts = new CancellationTokenSource(cancellationDucraion);
        var cancellationToken = cts.Token;
        var defaultEventsOptions = EvDbContinuousFetchOptions.ContinueIfEmpty;
        int count = BATCH_SIZE * 2;
        var startAt = DateTimeOffset.UtcNow.AddMinutes(-1);

        // produce messages before start listening to the change stream
        for (int i = 0; i < count; i += CHUNCK_SIZE)
        {
            await ProcuceStudentReceivedGradeAsync(CHUNCK_SIZE, i);
        }
        await Task.Delay(50); // Change stream ignore last ms

        EvDbShardName shard = EvDbNoViewsOutbox.DEFAULT_SHARD_NAME;
        IAsyncEnumerable<EvDbMessage> messages =
                        _changeStream.GetMessagesAsync(shard, startAt, defaultEventsOptions, cancellationToken);

        long lastOffset = 0;
        await foreach (var message in messages)
        {
            long messageOffset = message.StreamCursor.Offset;
            Assert.Equal(lastOffset, messageOffset - 1);
            lastOffset = messageOffset;

            AvgMessage data = JsonSerializer.Deserialize<AvgMessage>(message.Payload) ?? throw new Exception("Deserialize returned null");
            Assert.Equal(messageOffset, data!.Avg);

            if (messageOffset == 50)
            {
                var _ = ProcuceStudentReceivedGradeAsync(FUTURE_COUNT, count); // produce more messages after start listening to the change stream
            }
            if (messageOffset == count + FUTURE_COUNT)
                await cts.CancelAsync();
        }

        Assert.Equal(count + FUTURE_COUNT, lastOffset);
    }

    #endregion //  ChangeStream_GetMessages_Succeed

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