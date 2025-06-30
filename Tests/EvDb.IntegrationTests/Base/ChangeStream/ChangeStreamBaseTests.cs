// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using Cocona;
using EvDb.Core.Adapters;
using EvDb.Scenes;
using EvDb.UnitTests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Xunit.Abstractions;

public abstract class ChangeStreamBaseTests : BaseIntegrationTests
{
    private readonly IEvDbNoViews _stream;
    protected readonly IConfiguration _configuration;
    private readonly IEvDbNoViewsFactory _factory;
    private readonly IEvDbChangeStream _changeStream;
    private readonly Guid _streamId;

    #region Ctor

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

    #endregion //  Ctor

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
        var defaultEventsOptions = EvDbContinuousFetchOptions.ContinueWhenEmpty;
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
                        _changeStream.GetFromOutboxAsync(shard, startAt, defaultEventsOptions, cancellationToken);

        long lastOffset = 0;
        await foreach (var message in messages)
        {
            long messageOffset = message.StreamCursor.Offset;
            Assert.Equal(lastOffset, messageOffset - 1);
            lastOffset = messageOffset;

            if (message.MessageType == AvgMessage.PAYLOAD_TYPE)
            {
                AvgMessage data = JsonSerializer.Deserialize<AvgMessage>(message.Payload) ?? throw new Exception("Deserialize returned null");
                Assert.Equal(messageOffset, data!.Avg);
            }
            else if (message.MessageType == StudentPassedMessage.PAYLOAD_TYPE)
            {
                StudentPassedMessage data = JsonSerializer.Deserialize<StudentPassedMessage>(message.Payload) ?? throw new Exception("Deserialize returned null");
                Assert.Equal(88, data.StudentId);
            }
            else
            {
                throw new InvalidOperationException($"Unknown message type: {message.MessageType}");
            }

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

    #region ChangeStream_GetMessages_Filter_Channels

    [Fact]
    public virtual async Task ChangeStream_GetMessages_Filter_Channels()
    {
        const int BATCH_SIZE = 300;
        const int CHUNCK_SIZE = 40;
        var cancellationDucraion = Debugger.IsAttached
                                        ? TimeSpan.FromMinutes(10)
                                        : TimeSpan.FromSeconds(5);

        using var cts = new CancellationTokenSource(cancellationDucraion);
        var cancellationToken = cts.Token;
        var defaultEventsOptions = EvDbContinuousFetchOptions.ContinueWhenEmpty;
        int count = BATCH_SIZE * 2;
        var startAt = DateTimeOffset.UtcNow.AddMinutes(-1);

        // produce messages before start listening to the change stream
        for (int i = 0; i < count; i += CHUNCK_SIZE)
        {
            await ProcuceStudentReceivedGradeAsync(CHUNCK_SIZE, i);
        }
        await Task.Delay(50); // Change stream ignore last ms

        EvDbShardName shard = EvDbNoViewsOutbox.DEFAULT_SHARD_NAME;
        EvDbMessageFilter filter = EvDbMessageFilter.Create(startAt)
                                                    .AddChannel(AvgMessage.Channels.DEFAULT);
        IAsyncEnumerable<EvDbMessage> messages =
                        _changeStream.GetFromOutboxAsync(shard, filter, defaultEventsOptions, cancellationToken);

        await foreach (var message in messages)
        {
            if (message.MessageType == AvgMessage.PAYLOAD_TYPE)
            {
                AvgMessage data = JsonSerializer.Deserialize<AvgMessage>(message.Payload) ?? throw new Exception("Deserialize returned null");
                Assert.NotEqual(0, data!.Avg);
            }
            else
            {
                throw new InvalidOperationException($"Unknown message type: {message.MessageType}");
            }

            if (message.StreamCursor.Offset > count - 5)
                await cts.CancelAsync();
        }
    }

    #endregion //  ChangeStream_GetMessages_Filter_Channels

    #region ChangeStream_GetMessages_Filter_MessageTypes

    [Fact]
    public virtual async Task ChangeStream_GetMessages_Filter_MessageTypes()
    {
        const int BATCH_SIZE = 300;
        const int CHUNCK_SIZE = 40;
        var cancellationDucraion = Debugger.IsAttached
                                        ? TimeSpan.FromMinutes(10)
                                        : TimeSpan.FromSeconds(5);

        using var cts = new CancellationTokenSource(cancellationDucraion);
        var cancellationToken = cts.Token;
        var defaultEventsOptions = EvDbContinuousFetchOptions.ContinueWhenEmpty;
        int count = BATCH_SIZE * 2;
        var startAt = DateTimeOffset.UtcNow.AddMinutes(-1);

        // produce messages before start listening to the change stream
        for (int i = 0; i < count; i += CHUNCK_SIZE)
        {
            await ProcuceStudentReceivedGradeAsync(CHUNCK_SIZE, i);
        }
        await Task.Delay(50); // Change stream ignore last ms

        EvDbShardName shard = EvDbNoViewsOutbox.DEFAULT_SHARD_NAME;
        EvDbMessageFilter filter = EvDbMessageFilter.Create(startAt)
                                                    .AddMessageType(AvgMessage.PAYLOAD_TYPE);
        IAsyncEnumerable<EvDbMessage> messages =
                        _changeStream.GetFromOutboxAsync(shard, filter, defaultEventsOptions, cancellationToken);

        await foreach (var message in messages)
        {
            if (message.MessageType == AvgMessage.PAYLOAD_TYPE)
            {
                AvgMessage data = JsonSerializer.Deserialize<AvgMessage>(message.Payload) ?? throw new Exception("Deserialize returned null");
                Assert.NotEqual(0, data!.Avg);
            }
            else
            {
                throw new InvalidOperationException($"Unknown message type: {message.MessageType}");
            }

            if (message.StreamCursor.Offset > count - 5)
                await cts.CancelAsync();
        }
    }

    #endregion //  ChangeStream_GetMessages_Filter_MessageTypes

    #region ChangeStream_GetMessageRecords_Succeed

    [Fact]
    public virtual async Task ChangeStream_GetMessageRecords_Succeed()
    {
        const int BATCH_SIZE = 300;
        const int FUTURE_COUNT = 30;
        const int CHUNCK_SIZE = 40;
        var cancellationDucraion = Debugger.IsAttached
                                        ? TimeSpan.FromMinutes(10)
                                        : TimeSpan.FromSeconds(5);

        using var cts = new CancellationTokenSource(cancellationDucraion);
        var cancellationToken = cts.Token;
        var defaultEventsOptions = EvDbContinuousFetchOptions.ContinueWhenEmpty;
        int count = BATCH_SIZE * 2;
        var startAt = DateTimeOffset.UtcNow.AddMinutes(-1);

        // produce messages before start listening to the change stream
        for (int i = 0; i < count; i += CHUNCK_SIZE)
        {
            await ProcuceStudentReceivedGradeAsync(CHUNCK_SIZE, i);
        }
        await Task.Delay(50); // Change stream ignore last ms

        EvDbShardName shard = EvDbNoViewsOutbox.DEFAULT_SHARD_NAME;
        IAsyncEnumerable<EvDbMessageRecord> messages =
                        _changeStream.GetRecordsFromOutboxAsync(shard, startAt, defaultEventsOptions, cancellationToken);

        long lastOffset = 0;
        await foreach (var message in messages)
        {
            long messageOffset = message.Offset;
            Assert.Equal(lastOffset, messageOffset - 1);
            lastOffset = messageOffset;

            if (message.MessageType == AvgMessage.PAYLOAD_TYPE)
            {
                AvgMessage data = JsonSerializer.Deserialize<AvgMessage>(message.Payload) ?? throw new Exception("Deserialize returned null");
                Assert.Equal(messageOffset, data!.Avg);
            }
            else if (message.MessageType == StudentPassedMessage.PAYLOAD_TYPE)
            {
                StudentPassedMessage data = JsonSerializer.Deserialize<StudentPassedMessage>(message.Payload) ?? throw new Exception("Deserialize returned null");
                Assert.Equal(88, data.StudentId);
            }
            else
            {
                throw new InvalidOperationException($"Unknown message type: {message.MessageType}");
            }

            if (messageOffset == 50)
            {
                var _ = ProcuceStudentReceivedGradeAsync(FUTURE_COUNT, count); // produce more messages after start listening to the change stream
            }
            if (messageOffset == count + FUTURE_COUNT)
                await cts.CancelAsync();
        }

        Assert.Equal(count + FUTURE_COUNT, lastOffset);
    }

    #endregion //  ChangeStream_GetMessageRecords_Succeed

    #region ChangeStream_SubscribeToMessage_Succeed

    [Fact]
    public virtual async Task ChangeStream_SubscribeToMessage_Succeed()
    {
        const int BATCH_SIZE = 300;
        const int FUTURE_COUNT = 30;
        const int CHUNCK_SIZE = 40;
        const int BOUNDED_CAPACITY = 50; // check the back-pressure

        var cancellationDucraion = Debugger.IsAttached
                                        ? TimeSpan.FromMinutes(10)
                                        : TimeSpan.FromSeconds(5);

        using var cts = new CancellationTokenSource(cancellationDucraion);
        var cancellationToken = cts.Token;
        var defaultEventsOptions = EvDbContinuousFetchOptions.ContinueWhenEmpty;
        int count = BATCH_SIZE * 2;
        var startAt = DateTimeOffset.UtcNow.AddMinutes(-1);

        // produce messages before start listening to the change stream
        for (int i = 0; i < count; i += CHUNCK_SIZE)
        {
            await ProcuceStudentReceivedGradeAsync(CHUNCK_SIZE, i);
        }
        await Task.Delay(50); // Change stream ignore last ms

        int total = count + FUTURE_COUNT;
        EvDbShardName shard = EvDbNoViewsOutbox.DEFAULT_SHARD_NAME;
        long processingCounter = 0;
        var actionblock = new ActionBlock<EvDbMessage>(async message =>
        {
            var processed = Interlocked.Increment(ref processingCounter);

            if (processed == 50)
            {
                var _ = ProcuceStudentReceivedGradeAsync(FUTURE_COUNT, count); // produce more messages after start listening to the change stream
            }
            if (processed == total)
                await cts.CancelAsync();
        }, new ExecutionDataflowBlockOptions
        {
            CancellationToken = CancellationToken.None,
            MaxDegreeOfParallelism = 10,
            BoundedCapacity = BOUNDED_CAPACITY
        });
        Task subscription = _changeStream.SubscribeToMessageAsync(actionblock, shard, startAt, defaultEventsOptions, cancellationToken);

        await subscription; // push all into the action block
        await actionblock.Completion; // all completed

        Assert.Equal(total, processingCounter);
    }

    #endregion //  ChangeStream_SubscribeToMessage_Succeed

    #region ChangeStream_SubscribeToMessageRecords_Succeed

    [Fact]
    public virtual async Task ChangeStream_SubscribeToMessageRecords_Succeed()
    {
        const int BATCH_SIZE = 300;
        const int FUTURE_COUNT = 30;
        const int CHUNCK_SIZE = 40;
        const int BOUNDED_CAPACITY = 50; // check the back-pressure

        var cancellationDucraion = Debugger.IsAttached
                                        ? TimeSpan.FromMinutes(10)
                                        : TimeSpan.FromSeconds(5);

        using var cts = new CancellationTokenSource(cancellationDucraion);
        var cancellationToken = cts.Token;
        var defaultEventsOptions = EvDbContinuousFetchOptions.ContinueWhenEmpty;
        int count = BATCH_SIZE * 2;
        var startAt = DateTimeOffset.UtcNow.AddMinutes(-1);

        // produce messages before start listening to the change stream
        for (int i = 0; i < count; i += CHUNCK_SIZE)
        {
            await ProcuceStudentReceivedGradeAsync(CHUNCK_SIZE, i);
        }
        await Task.Delay(50); // Change stream ignore last ms

        int total = count + FUTURE_COUNT;
        EvDbShardName shard = EvDbNoViewsOutbox.DEFAULT_SHARD_NAME;
        long processingCounter = 0;
        var actionblock = new ActionBlock<EvDbMessageRecord>(async message =>
        {
            var processed = Interlocked.Increment(ref processingCounter);

            if (processed == 50)
            {
                var _ = ProcuceStudentReceivedGradeAsync(FUTURE_COUNT, count); // produce more messages after start listening to the change stream
            }
            if (processed == total)
                await cts.CancelAsync();
        }, new ExecutionDataflowBlockOptions
        {
            CancellationToken = CancellationToken.None,
            MaxDegreeOfParallelism = 10,
            BoundedCapacity = BOUNDED_CAPACITY
        });
        Task subscription = _changeStream.SubscribeToMessageRecordsAsync(actionblock, shard, startAt, defaultEventsOptions, cancellationToken);

        await subscription; // push all into the action block
        await actionblock.Completion; // all completed

        Assert.Equal(total, processingCounter);
    }

    #endregion //  ChangeStream_SubscribeToMessageRecords_Succeed

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