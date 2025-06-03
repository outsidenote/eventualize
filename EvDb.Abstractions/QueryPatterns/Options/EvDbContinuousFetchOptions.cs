namespace EvDb.Core;

//[GenerateBuilderPattern]
/// <summary>
/// Options for continuous fetch operations.
/// </summary>
public readonly record struct EvDbContinuousFetchOptions
{
    public static EvDbContinuousFetchOptions CompleteIfEmpty { get; } = new EvDbContinuousFetchOptions { CompleteWhenEmpty = true };
    public static EvDbContinuousFetchOptions ContinueIfEmpty { get; } = new EvDbContinuousFetchOptions();

    public EvDbContinuousFetchOptions()
    {
    }

    /// <summary>
    /// The underline batch size to fetch in a single request (for the continuous fetch operation).
    /// </summary>
    public int BatchSize { get; init; } = 300;

    /// <summary>
    /// When set to true, the continuous fetch operation will complete when there are no more items to fetch.
    /// </summary>
    public bool CompleteWhenEmpty { get; init; }

    /// <summary>
    /// Delay duration when the fetch operation returns no items (and CompleteWhenEmpty is false).
    /// </summary>
    public DelayStrategy DelayWhenEmpty { get; init; }

    ///// <summary>
    ///// Preferences for notification vs. batch polling in continuous fetch operations.
    ///// </summary>
    //public EvDbNotificationPreferences NotificationPreferences  { get; init; }

    #region WithBatchSize

    public EvDbContinuousFetchOptions WithBatchSize(int batchSize)
    {
        return this with { BatchSize = batchSize };
    }

    #endregion //  WithBatchSize
}
