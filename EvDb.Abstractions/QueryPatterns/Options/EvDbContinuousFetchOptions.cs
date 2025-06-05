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
        DelayWhenEmpty = new DelayStrategy();
    }

    /// <summary>
    /// When set to true, the continuous fetch operation will complete when there are no more items to fetch.
    /// </summary>
    public bool CompleteWhenEmpty { get; init; }

    /// <summary>
    /// Delay duration when the fetch operation returns no items (and CompleteWhenEmpty is false).
    /// </summary>
    public DelayStrategy DelayWhenEmpty { get; init; }
}
