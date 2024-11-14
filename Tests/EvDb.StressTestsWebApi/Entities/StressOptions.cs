namespace EvDb.StressTestsWebApi;

public readonly record struct StressOptions(
        int WriteCycleCount,
        int StreamsCount,
        int DegreeOfParallelismPerStream,
        int BatchSize)
{
    public string StreamPrefix { get; init; } = $"{DateTime.UtcNow}:yyyy_MM_dd_HH_mm";
}
