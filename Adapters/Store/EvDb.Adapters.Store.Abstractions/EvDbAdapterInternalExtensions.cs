using EvDb.Core;
using EvDb.Core.Adapters;

namespace EvDb.Adapters.Internals;

public static class EvDbAdapterInternalExtensions
{

    #region DelayWhenEmptyAsync

    public static async Task<(TimeSpan Delay, int AttemptsWhenEmpty, bool ShouldExit)> DelayWhenEmptyAsync(
                                                        this EvDbContinuousFetchOptions options,
                                                        bool reachTheEnd,
                                                        TimeSpan delay,
                                                        int attemptsWhenEmpty,
                                                        CancellationToken cancellation)
    {
        DelayStrategy whenEmpty = options.DelayWhenEmpty;
        if (!reachTheEnd)
        {
            return (whenEmpty.StartDuration, 0, false);
        }
        if (options.CompleteWhenEmpty)
        {
            // If we have no rows and the options say to stop, we break the loop
            return (delay, attemptsWhenEmpty, true);
        }
        if (attemptsWhenEmpty != 0)
            delay = whenEmpty.IncrementalLogic(delay, attemptsWhenEmpty);
        if (delay > whenEmpty.MaxDuration)
            delay = whenEmpty.MaxDuration;
        await Task.Delay(delay, cancellation).SwallowCancellationAsync();

        attemptsWhenEmpty++;
        return (delay, attemptsWhenEmpty, false);
    }

    #endregion //  DelayWhenEmptyAsync

}
