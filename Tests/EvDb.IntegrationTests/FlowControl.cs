//[assembly: CollectionBehavior(MaxParallelThreads = 15)]
//[assembly: CollectionBehavior(DisableTestParallelization = true)]

using Castle.Components.DictionaryAdapter;

[CollectionDefinition("sink", DisableParallelization = false)]
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