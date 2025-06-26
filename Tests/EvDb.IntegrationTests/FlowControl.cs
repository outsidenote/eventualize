//[assembly: CollectionBehavior(MaxParallelThreads = 15)]
//[assembly: CollectionBehavior(DisableTestParallelization = true)]

[CollectionDefinition("Sink")]
public abstract class SinkDefinition
{
}

[CollectionDefinition("stress", DisableParallelization = true)]
public abstract class StressDefinition
{
}

[CollectionDefinition("otel", DisableParallelization = true)]
public abstract class OtelDefinition
{
}