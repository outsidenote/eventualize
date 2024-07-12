namespace EvDb.Core.Tests;

using Cocona;
using EvDb.MinimalStructure;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit.Abstractions;

public class TimeProviderTests
{
    private readonly IEvDbDemoStreamFactory _factory;
    private readonly IEvDbStorageAdapter _storageAdapter = A.Fake<IEvDbStorageAdapter>();
    private readonly ITestOutputHelper _output;
    private static string GenerateStreamId() => $"test-stream-{Guid.NewGuid():N}";
    private readonly TimeProvider _timeProvider = A.Fake<TimeProvider>();

    public TimeProviderTests(ITestOutputHelper output)
    {
        _output = output;
        var builder = CoconaApp.CreateBuilder();
        var services = builder.Services;
        services.AddSingleton(_storageAdapter);
        services.AddEvDbDemoStreamFactory();
        services.AddSingleton<TimeProvider>(_timeProvider);
        var sp = services.BuildServiceProvider();
        _factory = sp.GetRequiredService<IEvDbDemoStreamFactory>();
    }

    [Fact]
    public async Task Stream_WhenAddingPendingEvent_HonorTimeProvider_Test()
    {
        #region TimeProvider timeProvider = A.Fake<TimeProvider>()

        DateTimeOffset seed = DateTimeOffset.UtcNow;
        int i = 0;
        A.CallTo(() => _timeProvider.GetUtcNow())
            .ReturnsLazily(() =>
            {
                int local = i / 2; // both stream and view call it
                i++;
                int sec = local * 10;
                if (local == 3)
                    sec = 25;
                return seed.AddSeconds(sec);
            });

        #endregion // TimeProvider _timeProvider = A.Fake<TimeProvider>()

        var streamId = GenerateStreamId();
        var stream = _factory.Create(streamId);
        for (int k = 0; k < 4; k++)
        {
            await stream.AddAsync(new Event1(1, $"Person {k}", k));

        }

        ThenPendingEventsAddedSuccessfully();

        void ThenPendingEventsAddedSuccessfully()
        {
            var events = (stream as IEvDbStreamStoreData)!.Events.ToArray();
            Assert.Equal(4, events.Length);
            for (int j = 0; j < 4; j++)
            {
                int expectedSec = j == 3 ? 25 : j * 10;
                Assert.Equal(seed.AddSeconds(expectedSec), events[j].CapturedAt);
            }
            Assert.Equal(5, stream.Views.Interval);
        }
    }
}