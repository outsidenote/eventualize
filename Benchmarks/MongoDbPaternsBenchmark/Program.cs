using BenchmarkDotNet.Running;
using MongoBenchmark;
using System.Diagnostics;

await ManualAsync();

//Benchmark();
Console.ReadKey(false);

static void Benchmark()
{
    var summary = BenchmarkRunner.Run<MongoAccessPatternsBenchmarks>();
    Console.WriteLine(summary.Table);
}

static async Task ManualAsync()
{
    var x = new MongoAccessPatternsBenchmarks();

    for (int i = 0; i < 20; i++)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Compound");
        Console.ResetColor();

        x.GlobalSetup();
        x.IterationSetup();
        var sw = Stopwatch.StartNew();
        await x.Compound_GetBatch();
        sw.Stop();
        var complexDuration = sw.Elapsed.TotalSeconds;
        x.GlobalCleanup();


        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("By Id");
        Console.ResetColor();

        x.GlobalSetup();
        x.IterationSetup();
        sw = Stopwatch.StartNew();
        await x.ById_GetBatch();
        sw.Stop();
        var byIdDuration = sw.Elapsed.TotalSeconds;
        x.GlobalCleanup();


        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("By Id Via Cursor");
        Console.ResetColor();

        x.GlobalSetup();
        x.IterationSetup();
        sw = Stopwatch.StartNew();
        await x.ById_GetViaCursor();
        sw.Stop();
        var byIdViaCursorDuration = sw.Elapsed.TotalSeconds;
        x.GlobalCleanup();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"""
    Compound:           {complexDuration:N2}
    By Id:              {byIdDuration:N2}
    By Id via Cursor:   {byIdViaCursorDuration:N2}
    """);
        Console.ResetColor();
        Console.WriteLine("====================================");
    }
}
