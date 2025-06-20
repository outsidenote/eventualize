﻿namespace EvDb.Core;

public readonly record struct DelayStrategy
{
    public TimeSpan DefaultNextDelay(TimeSpan lastDelay, int attempt) =>
                        attempt switch
                        {
                            <= 5 => StartDuration,
                            _ when lastDelay.TotalMicroseconds == 0 => TimeSpan.FromMilliseconds(50),
                            _ => TimeSpan.FromMicroseconds(lastDelay.TotalMicroseconds * 2)
                        };

    public DelayStrategy()
    {
        StartDuration = TimeSpan.FromMilliseconds(200); // Default delay when empty
        MaxDuration = TimeSpan.FromSeconds(5); // Default max duration
        IncrementalLogic = DefaultNextDelay; // Default incremental logic
    }

    /// <summary>
    /// First delay duration when the fetch operation returns no items (and CompleteWhenEmpty is false).
    /// Increases exponentially after few attempt until it reaches MaxDuration, unless having a custom incremental logic.
    /// </summary>
    public TimeSpan StartDuration { get; init; }

    /// <summary>
    /// Delay duration when the fetch operation returns no items (and CompleteWhenEmpty is false).
    /// </summary>
    public TimeSpan MaxDuration { get; init; }

    /// <summary>
    /// Custom incremental logic to calculate the next delay duration 
    /// based on the previous delay and the number of attempts.
    /// </summary>
    public Func<TimeSpan, int, TimeSpan> IncrementalLogic { get; init; }
}
