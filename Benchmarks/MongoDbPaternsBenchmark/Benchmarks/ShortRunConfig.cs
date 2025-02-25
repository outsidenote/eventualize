using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Validators;

namespace MongoBenchmark;

// A custom configuration that runs with a minimal warmup and iteration count.
// This ensures the entire benchmark runs for less than a minute.
public class ShortRunConfig : ManualConfig
{
    public ShortRunConfig()
    {
        // Use a simple job with minimal iterations.
        //AddJob(Job.Default
        //    .WithLaunchCount(1)
        //    .WithWarmupCount(1)
        //    .WithIterationCount(8)
        //    .WithInvocationCount(16))
        //    .AddValidator(ExecutionValidator.FailOnError); // Adjust invocation count if necessary.
        //AddExporter(RPlotExporter.Default, MarkdownExporter.Console);
        AddExporter(MarkdownExporter.Console);

        //AddColumn(TargetMethodColumn.Method, StatisticColumn.Max);
        //AddColumn(new TagColumn("Kind", name => name.Substring(0, 3)));
        //AddColumn(new TagColumn("Number", name => name.Substring(3)));
        //AddColumn(new TagColumn("Median ", name => name));
        //AddExporter(MarkdownExporter.GitHub); // Exports a GitHub-flavored Markdown table.
    }
}
