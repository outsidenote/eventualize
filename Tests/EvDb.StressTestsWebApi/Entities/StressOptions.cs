namespace EvDb.StressTestsWebApi;

public readonly record struct StressOptions(
        int WriteCycleCount,
        int StreamsCount,
        int DegreeOfParallelismPerStream,
        int BatchSize);
