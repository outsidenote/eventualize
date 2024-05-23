namespace EvDb.Core.Tests;

using EvDb.Scenes;
using EvDb.UnitTests;
using FakeItEasy;
using Xunit.Abstractions;

public class TimeProviderTests
{
    private readonly IEvDbStorageAdapter _storageAdapter = A.Fake<IEvDbStorageAdapter>();
    private readonly ITestOutputHelper _output;

    public TimeProviderTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Stream_WhenAddingPendingEvent_HonorTimeProvider()
    {
        #region TimeProvider timeProvider = A.Fake<TimeProvider>()

        TimeProvider timeProvider = A.Fake<TimeProvider>();
        DateTimeOffset seed = DateTimeOffset.UtcNow;
        int i = 0;
        A.CallTo(() => timeProvider.GetUtcNow())
            .ReturnsLazily(() =>
            {
                int local = i / 2; // both stream and view call it
                i++;
                int sec = local * 10;
                if(local == 3)
                    sec = 25;
                return seed.AddSeconds(sec);
            });

        #endregion // TimeProvider timeProvider = A.Fake<TimeProvider>()

        IEvDbSchoolStream stream = _storageAdapter
                                .GivenLocalStreamWithPendingEvents(_output, timeProvider: timeProvider);

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
            Assert.Equal(5, stream.Views.MinInterval);
        }
    }
}