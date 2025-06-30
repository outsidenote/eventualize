// Ignore Spelling: Sql

namespace EvDb.Core.Tests;
using EvDb.Core.Adapters;
using EvDb.Scenes;
using EvDb.UnitTests;
using OpenTelemetry;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Text.Json;
using Xunit.Abstractions;

[Collection("otel")]
public abstract class StreamOtelBaseTests : BaseIntegrationTests
{
    private readonly TracerProvider _tracerProvider;
    private readonly ActivitySource TraceSource = new ActivitySource("Test");

    protected StreamOtelBaseTests(ITestOutputHelper output, StoreType storeType) :
        base(output, storeType)
    {
        _tracerProvider = Sdk.CreateTracerProviderBuilder()
         .SetSampler<AlwaysOnSampler>()
         .AddSource(TraceSource.Name)
         .Build();

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
        using var activity = TraceSource.StartActivity("test-scope");

        var streamId = Steps.GenerateStreamId();
        IEvDbSchoolStream stream = await StorageContext
                            .GivenLocalStreamWithPendingEvents(_storeType, TestingStreamStore, streamId: streamId)
                            .WhenStreamIsSavedAsync();

        await ThenStreamSavedWithoutSnapshot();

        async Task ThenStreamSavedWithoutSnapshot()
        {

            ICollection<EvDbMessageRecord> defaultscommandsCollection = await GetOutboxAsync(EvDbShardName.Default).ToEnumerableAsync();
            EvDbMessageRecord[] defaults = defaultscommandsCollection!.ToArray();

            for (int i = 1; i <= defaults.Length; i++)
            {
                EvDbMessageRecord item = defaults[i - 1];
                item.AssertTelemetryContextEquals(activity);
            }
        }
    }

    public override Task DisposeAsync()
    {
        _tracerProvider.Dispose();
        return Task.CompletedTask;
    }
}