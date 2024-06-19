namespace EvDb.StressTestsWebApi;

public readonly record struct StressOptions(
        int writeCycleCount,
        int streamsCount,
        int degreeOfParallelismPerStream,
        int batchSize);
