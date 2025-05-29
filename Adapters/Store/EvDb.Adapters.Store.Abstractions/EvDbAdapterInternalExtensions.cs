using EvDb.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvDb.Adapters.Internals;

public static class EvDbAdapterInternalExtensions
{

    #region DelayWhenEmptyAsync

    public static async Task<(TimeSpan Delay, int attemptsWhenEmpty, bool ShouldExit)> DelayWhenEmptyAsync(
                                                        this EvDbContinuousFetchOptions options,
                                                        bool hasRows,
                                                        TimeSpan delay,
                                                        int attemptsWhenEmpty,
                                                        CancellationToken cancellation)
    {
        DelayStrategy whenEmpty = options.DelayWhenEmpty;
        if (hasRows)
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
        await Task.Delay(delay, cancellation);

        attemptsWhenEmpty++;
        return (delay, attemptsWhenEmpty, false);
    }

    #endregion //  DelayWhenEmptyAsync

}
